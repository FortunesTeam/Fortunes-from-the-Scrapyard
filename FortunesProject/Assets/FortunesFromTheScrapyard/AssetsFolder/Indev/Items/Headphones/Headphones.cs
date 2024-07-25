using RoR2;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using MSU;
using R2API;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    public class Headphones : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_HEADPHONES_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        public static float chanceBase = 10f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float headphoneBaseDamage = 1.2f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float headphoneDamageStack = 1.2f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float headphoneRaduisBase = 10f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float headphoneRaduisStack = 2f;

        public static GameObject headphonesShockwavePrefab;

        public override void Initialize()
        {
            headphonesShockwavePrefab = AssetCollection.FindAsset<GameObject>("HeadphoneShockwaveEffect");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acHeadphones", ScrapyardBundle.Indev);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public class HeadphonesBehaviour : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.Headphones;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (!NetworkServer.active) { return; }
                CharacterBody attackerBody = damageReport.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = damageReport.victimBody;
                if (attackerBody)
                {
                    if (victimBody.HasBuff(ScrapyardContent.Buffs.bdDisorient))
                    {
                        victimBody.RemoveBuff(ScrapyardContent.Buffs.bdDisorient);

                        BlastAttack blastAttack = new BlastAttack();

                        blastAttack.procCoefficient = 0.2f;
                        blastAttack.attacker = attackerBody.gameObject;
                        blastAttack.inflictor = null;
                        blastAttack.teamIndex = attackerBody.teamComponent.teamIndex;
                        blastAttack.baseDamage = damageReport.damageDealt * Headphones.GetStackValue(headphoneBaseDamage, headphoneDamageStack, attackerBody.inventory.GetItemCount(ScrapyardContent.Items.Headphones));
                        blastAttack.baseForce = 100f;
                        blastAttack.position = base.transform.position;
                        blastAttack.radius = 15f + Headphones.GetStackValue(headphoneRaduisBase, headphoneRaduisStack, attackerBody.inventory.GetItemCount(ScrapyardContent.Items.Headphones));
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        // blastAttack.bonusForce = Vector3.zero;
                        blastAttack.damageType = DamageType.Shock5s;
                        blastAttack.Fire();
                        
                        EffectManager.SpawnEffect(headphonesShockwavePrefab, new EffectData
                        {
                            origin = victimBody.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1
                        }, false);

                    }
                    else if (!victimBody.HasBuff(ScrapyardContent.Buffs.bdDisorient))
                    {
                        float procChance = chanceBase * damageReport.damageInfo.procCoefficient;
                        float adjustedProcChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
                        if (Util.CheckRoll(adjustedProcChance, damageReport.attackerMaster))
                        {
                            victimBody.AddBuff(ScrapyardContent.Buffs.bdDisorient);
                        }
                    }
                }
            }
        }
    }
}
