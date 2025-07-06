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
    public class BrokenHeadphones : FFTSItem
    {
        public const string TOKEN = "FFTS_ITEM_BROKENHEADPHONES_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        public static float chanceBase = 10f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float headphoneBaseDamage = 1.2f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float headphoneDamageStack = 1.2f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float headphoneRadiusBase = 10f;
        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float headphoneRadiusStack = 2f;

        public static GameObject headphonesShockwavePrefab;

        public static GameObject headphonesVisualEffect;
        public override void Initialize()
        {
            FFTSContent.ProcType.BrokenHeadphones = ProcTypeAPI.ReserveProcType();

            headphonesShockwavePrefab = assetCollection.FindAsset<GameObject>("BrokenHeadphonesShockwaveEffect");

            headphonesVisualEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeEffect.prefab").WaitForCompletion().InstantiateClone("BrokenHeadphonesVisualEffect", false);

            //Needs better texture
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", assetCollection.FindAsset<Texture>("texSwirl"));
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHook.png").WaitForCompletion());
            headphonesVisualEffect.transform.Find("Visual, On").Find("PulseEffect, Ring").gameObject.GetComponent<ParticleSystemRenderer>().material.SetFloat("_AlphaBoost", 20);

            bool tempAdd(CharacterBody body) => body.HasBuff(FFTSContent.Buffs.bdBrokenHeadphonesDisorient);
            TempVisualEffectAPI.AddTemporaryVisualEffect(headphonesVisualEffect, tempAdd);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acBrokenHeadphones", FFTSBundle.Items);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public class HeadphonesBehaviour : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => FFTSContent.Items.BrokenHeadphones;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (!NetworkServer.active) { return; }

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                if (attackerBody && !damageInfo.procChainMask.HasModdedProc(FFTSContent.ProcType.BrokenHeadphones))
                {
                    if (victimHealthComponent.body.HasBuff(FFTSContent.Buffs.bdBrokenHeadphonesDisorient) && damageInfo.dotIndex == DotController.DotIndex.None)
                    {
                        victimHealthComponent.body.RemoveBuff(FFTSContent.Buffs.bdBrokenHeadphonesDisorient);

                        BlastAttack blastAttack = new BlastAttack();

                        blastAttack.procCoefficient = 0.2f;
                        blastAttack.attacker = attackerBody.gameObject;
                        blastAttack.inflictor = null;
                        blastAttack.teamIndex = attackerBody.teamComponent.teamIndex;
                        blastAttack.baseDamage = attackerBody.damage * GetStackValue(headphoneBaseDamage, headphoneDamageStack, attackerBody.GetItemCount(GetItemDef()));
                        blastAttack.baseForce = 100f;
                        blastAttack.position = damageInfo.position;
                        blastAttack.radius = GetStackValue(headphoneRadiusBase, headphoneRadiusStack, attackerBody.GetItemCount(GetItemDef()));
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        // blastAttack.bonusForce = Vector3.zero;
                        blastAttack.damageType = DamageType.Shock5s;
                        blastAttack.damageColorIndex = DamageColorIndex.Item;
                        blastAttack.procChainMask.AddModdedProc(FFTSContent.ProcType.BrokenHeadphones);
                        blastAttack.Fire();

                        EffectManager.SpawnEffect(headphonesShockwavePrefab, new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Quaternion.identity,
                            scale = 1f
                        }, true);

                    }
                    else if (!victimHealthComponent.body.HasBuff(FFTSContent.Buffs.bdBrokenHeadphonesDisorient))
                    {
                        float procChance = chanceBase * damageInfo.procCoefficient;
                        if (Util.CheckRoll(procChance, attackerBody.master))
                        {
                            victimHealthComponent.body.AddBuff(FFTSContent.Buffs.bdBrokenHeadphonesDisorient);
                        }
                    }
                }
            }
        }
    }
}
