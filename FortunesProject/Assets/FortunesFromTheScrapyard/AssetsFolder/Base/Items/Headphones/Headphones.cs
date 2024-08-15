using RoR2;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using MSU;
using RoR2.UI;
using R2API;
using UnityEngine;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

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

        public static GameObject headphonesVisualEffect;

        public static DamageAPI.ModdedDamageType HeadphonesProc;
        public override void Initialize()
        {
            HeadphonesProc = DamageAPI.ReserveDamageType();

            headphonesShockwavePrefab = AssetCollection.FindAsset<GameObject>("HeadphoneShockwaveEffect");

            headphonesVisualEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeEffect.prefab").WaitForCompletion().InstantiateClone("HeadphonesVisualEffect", false);

            //Needs better texture
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", AssetCollection.FindAsset<Texture>("texSwirl"));
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHook.png").WaitForCompletion());
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetFloat("_AlphaBoost", 20);

            bool tempAdd(CharacterBody body) => body.HasBuff(ScrapyardContent.Buffs.bdDisorient);
            TempVisualEffectAPI.AddTemporaryVisualEffect(headphonesVisualEffect, tempAdd);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acHeadphones", ScrapyardBundle.Items);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public class HeadphonesBehaviour : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.Headphones;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (!NetworkServer.active) { return; }

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                if (attackerBody && !damageInfo.HasModdedDamageType(HeadphonesProc))
                {
                    if (victimHealthComponent.body.HasBuff(ScrapyardContent.Buffs.bdDisorient) && damageInfo.dotIndex == DotController.DotIndex.None)
                    {
                        victimHealthComponent.body.RemoveBuff(ScrapyardContent.Buffs.bdDisorient);

                        BlastAttack blastAttack = new BlastAttack();

                        blastAttack.procCoefficient = 0.2f;
                        blastAttack.attacker = attackerBody.gameObject;
                        blastAttack.inflictor = null;
                        blastAttack.teamIndex = attackerBody.teamComponent.teamIndex;
                        blastAttack.baseDamage = attackerBody.damage * GetStackValue(headphoneBaseDamage, headphoneDamageStack, attackerBody.GetItemCount(GetItemDef()));
                        blastAttack.baseForce = 100f;
                        blastAttack.position = damageInfo.position;
                        blastAttack.radius = GetStackValue(headphoneRaduisBase, headphoneRaduisStack, attackerBody.GetItemCount(GetItemDef()));
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        // blastAttack.bonusForce = Vector3.zero;
                        blastAttack.damageType = DamageType.Shock5s;
                        blastAttack.damageColorIndex = DamageColorIndex.Item;
                        blastAttack.AddModdedDamageType(HeadphonesProc);
                        blastAttack.Fire();
                        
                        EffectManager.SpawnEffect(headphonesShockwavePrefab, new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Quaternion.identity,
                            scale = 1f
                        }, true);

                    }
                    else if (!victimHealthComponent.body.HasBuff(ScrapyardContent.Buffs.bdDisorient))
                    {
                        float procChance = chanceBase * damageInfo.procCoefficient;
                        if (Util.CheckRoll(procChance, attackerBody.master))
                        {
                            victimHealthComponent.body.AddBuff(ScrapyardContent.Buffs.bdDisorient);
                        }
                    }
                }
            }
        }
    }
}
