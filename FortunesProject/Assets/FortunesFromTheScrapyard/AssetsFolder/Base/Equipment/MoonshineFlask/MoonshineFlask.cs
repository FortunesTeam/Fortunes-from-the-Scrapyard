using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;
using R2API;
using UnityEngine.AddressableAssets;
using TMPro;
using static AkMIDIEvent;

namespace FortunesFromTheScrapyard.Equipments
{
    public sealed class MoonshineFlask : ScrapyardEquipment
    {
        public const string TOKEN = "SCRAPYARD_EQUIP_MOONSHINE_DESC";

        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 0)]
        public static float chanceToHit = 50f;

        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float basePercentageSaved = 0.6f;

        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 2)]
        public static float baseRadius = 2.5f;

        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 3)]
        public static float buffDuration = 15f;

        public static GameObject moonShineEffect;

        public static GameObject missEffect;

        public static GameObject explosionEffect;

        public static DamageAPI.ModdedDamageType MoonshineProc;
        public override bool Execute(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            SkillLocator skill = body.skillLocator;
            if (skill != null)
            {
                if (NetworkServer.active)
                {
                    body.AddTimedBuff(ScrapyardContent.Buffs.bdMoonshineFlask, buffDuration);
                }
                Util.PlaySound("sfx_moonshine_use", body.gameObject);
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            MoonshineProc = DamageAPI.ReserveDamageType();

            moonShineEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tonic/TonicBuffEffect.prefab").WaitForCompletion().InstantiateClone("MoonshinePrefab", false);

            bool tempAdd(CharacterBody body) => body.HasBuff(ScrapyardContent.Buffs.bdMoonshineFlask);
            TempVisualEffectAPI.AddTemporaryVisualEffect(moonShineEffect, tempAdd);

            missEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/BearProc.prefab").WaitForCompletion().InstantiateClone("MissProc");
            missEffect.EnsureComponent<NetworkIdentity>();
            EffectComponent effect = missEffect.EnsureComponent<EffectComponent>();
            effect.soundName = "sfx_moonshine_miss";
            effect.positionAtReferencedTransform = true;
            effect.parentToReferencedTransform = true;
            missEffect.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>().token = "SCRAPYARD_EQUIP_MOONSHINE_POPUP";

            EffectDef missEffectDef = new EffectDef(missEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(missEffectDef);

            explosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombExplosion.prefab").WaitForCompletion().InstantiateClone("MoonshineExplosionEffect", false);
            EffectComponent ex = explosionEffect.EnsureComponent<EffectComponent>();
            ex.soundName = "Play_lunar_wisp_attack2_explode";
            ex.applyScale = true;

            EffectDef explosionEffectDef = new EffectDef(explosionEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(explosionEffectDef);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<EquipmentAssetCollection>("acMoonshineFlask", ScrapyardBundle.Equipments);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class MoonshineBuffBehaviour : BaseBuffBehaviour, IOnIncomingDamageOtherServerReciever
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => ScrapyardContent.Buffs.bdMoonshineFlask;

            private float savedDamage;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            { 
                if(damageInfo.dotIndex == DotController.DotIndex.None && !damageInfo.HasModdedDamageType(MoonshineProc)) 
                {
                    if (CharacterBody.HasBuff(GetBuffDef()) && !Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(chanceToHit / CharacterBody.GetBuffCount(GetBuffDef())), CharacterBody.master.luck))
                    {
                        EffectManager.SpawnEffect(effectData: new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                        }, effectPrefab: missEffect, transmit: true);

                        damageInfo.rejected = true;

                        savedDamage += damageInfo.damage * basePercentageSaved;

                        CharacterBody.AddBuff(ScrapyardContent.Buffs.bdMoonshineStack);
                    }
                    else if (CharacterBody.HasBuff(ScrapyardContent.Buffs.bdMoonshineStack))
                    {
                        int buffCount = CharacterBody.GetBuffCount(ScrapyardContent.Buffs.bdMoonshineStack);
                        if (buffCount > 0 && damageInfo.procCoefficient != 0f)
                        {
                            float radius = (baseRadius + baseRadius * (float)buffCount) * damageInfo.procCoefficient;

                            EffectManager.SpawnEffect(explosionEffect, new EffectData
                            {
                                origin = damageInfo.position,
                                scale = radius,
                                rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                            }, transmit: true);

                            BlastAttack blastAttack = new BlastAttack
                            {
                                position = damageInfo.position,
                                baseDamage = savedDamage,
                                baseForce = 0f,
                                radius = radius,
                                attacker = damageInfo.attacker,
                                inflictor = null
                            };
                            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                            blastAttack.crit = damageInfo.crit;
                            blastAttack.procChainMask = damageInfo.procChainMask;
                            blastAttack.procCoefficient = 0f;
                            blastAttack.damageColorIndex = DamageColorIndex.Item;
                            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                            blastAttack.damageType = damageInfo.damageType;
                            blastAttack.AddModdedDamageType(MoonshineProc);
                            blastAttack.Fire();

                            CharacterBody.SetBuffCount(ScrapyardContent.Buffs.bdMoonshineStack.buffIndex, 0);
                            savedDamage = 0f;
                        }
                    }
                }
            }
        }
    }
}