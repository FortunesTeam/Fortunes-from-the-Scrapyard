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
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float chanceBase = 10f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float disorientDamage = 0.2f;

        public static float sprayDamage = 2.5f;

        public static float sprayRaduisBase = 10f;
        public static float sprayRaduisStack = 2f;

        public override void Initialize()
        {
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

                        blastAttack.attacker = attackerBody.gameObject;
                        blastAttack.inflictor = null;
                        blastAttack.teamIndex = attackerBody.teamComponent.teamIndex;
                        blastAttack.baseDamage = damageReport.damageDealt * Headphones.GetStackValue(sprayDamage, sprayDamage, attackerBody.inventory.GetItemCount(ScrapyardContent.Items.Headphones));
                        blastAttack.baseForce = 100f;
                        blastAttack.position = base.transform.position;
                        blastAttack.radius = 15f + Headphones.GetStackValue(sprayRaduisBase, sprayRaduisStack, attackerBody.inventory.GetItemCount(ScrapyardContent.Items.Headphones));
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        // blastAttack.bonusForce = Vector3.zero;
                        blastAttack.damageType = DamageType.Generic;
                        blastAttack.Fire();
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
