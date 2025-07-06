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
using static R2API.DamageAPI;

namespace FortunesFromTheScrapyard.Equipments
{
    public sealed class MoonshineFlask : FFTSEquipment
    {
        public const string TOKEN = "FFTS_EQUIP_MOONSHINE_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.DivideByN, 2, 0)]
        public static float chanceToHit = 100f;

        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float basePercentageSaved = 0.75f;

        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 2)]
        public static float baseRadius = 2.5f;

        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 3)]
        public static float buffDuration = 15f;

        public static GameObject moonShineEffect;

        public static GameObject missEffect;

        public static GameObject explosionEffect;
        public override bool Execute(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            SkillLocator skill = body.skillLocator;
            if (skill != null)
            {
                if (NetworkServer.active)
                {
                    body.AddTimedBuff(FFTSContent.Buffs.bdMoonshineFlask, buffDuration);
                }
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            FFTSContent.ProcType.MoonshineFlask = ProcTypeAPI.ReserveProcType();

            moonShineEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tonic/TonicBuffEffect.prefab").WaitForCompletion().InstantiateClone("MoonshinePrefab", false);

            bool tempAdd(CharacterBody body) => body.HasBuff(FFTSContent.Buffs.bdMoonshineFlask);
            TempVisualEffectAPI.AddTemporaryVisualEffect(moonShineEffect, tempAdd);

            missEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bear/BearProc.prefab").WaitForCompletion().InstantiateClone("MissProc", true);
            missEffect.EnsureComponent<NetworkIdentity>();
            EffectComponent effect = missEffect.EnsureComponent<EffectComponent>();
            effect.soundName = "sfx_moonshine_miss";
            effect.positionAtReferencedTransform = true;
            effect.parentToReferencedTransform = true;
            missEffect.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>().token = "FFTS_EQUIP_MOONSHINE_POPUP";

            EffectDef missEffectDef = new EffectDef(missEffect);

            FFTSContent.scrapyardContentPack.effectDefs.AddSingle(missEffectDef);

            explosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombExplosion.prefab").WaitForCompletion().InstantiateClone("MoonshineExplosionEffect", false);
            EffectComponent ex = explosionEffect.EnsureComponent<EffectComponent>();
            ex.soundName = "Play_lunar_wisp_attack2_explode";
            ex.applyScale = true;

            EffectDef explosionEffectDef = new EffectDef(explosionEffect);

            FFTSContent.scrapyardContentPack.effectDefs.AddSingle(explosionEffectDef);

            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess; ;
        }

        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody attackerBody = null;
            if (damageInfo.attacker) attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            MoonshineBehaviour moonshineComponent = null;
            if (attackerBody) moonshineComponent = attackerBody.GetComponent<MoonshineBehaviour>();

            if (attackerBody && damageInfo.attacker && moonshineComponent && (damageInfo.dotIndex & DotController.DotIndex.None) != 0 && !damageInfo.procChainMask.HasModdedProc(FFTSContent.ProcType.MoonshineFlask))
            {
                if (attackerBody.HasBuff(FFTSContent.Buffs.bdMoonshineFlask) && !Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(chanceToHit / attackerBody.GetBuffCount(FFTSContent.Buffs.bdMoonshineFlask)), attackerBody.master.luck))
                {
                    EffectManager.SpawnEffect(effectData: new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                    }, effectPrefab: missEffect, transmit: false);

                    damageInfo.rejected = true;

                    moonshineComponent.savedDamage += damageInfo.damage * basePercentageSaved;

                    attackerBody.AddBuff(FFTSContent.Buffs.bdMoonshineStack);
                }
                else if (attackerBody.HasBuff(FFTSContent.Buffs.bdMoonshineStack))
                {
                    int buffCount = attackerBody.GetBuffCount(FFTSContent.Buffs.bdMoonshineStack);
                    float radius = (baseRadius + baseRadius * (float)buffCount);

                    EffectManager.SpawnEffect(explosionEffect, new EffectData
                    {
                        origin = damageInfo.position,
                        scale = radius,
                        rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                    }, transmit: true);

                    BlastAttack blastAttack = new BlastAttack
                    {
                        position = damageInfo.position,
                        baseDamage = moonshineComponent.savedDamage,
                        baseForce = 200f,
                        radius = radius,
                        attacker = damageInfo.attacker,
                        inflictor = damageInfo.inflictor,
                        teamIndex = TeamComponent.GetObjectTeam(damageInfo.attacker),
                        crit = damageInfo.crit,
                        procChainMask = damageInfo.procChainMask,
                        procCoefficient = damageInfo.procCoefficient,
                        damageColorIndex = DamageColorIndex.Item,
                        falloffModel = BlastAttack.FalloffModel.None,
                        damageType = damageInfo.damageType,
                    };
                    blastAttack.procChainMask.AddModdedProc(FFTSContent.ProcType.MoonshineFlask);
                    /*var d = DamageAPI.GetModdedDamageTypeHoglder(damageInfo);
                    d.CopyTo(blastAttack);*/
                    blastAttack.Fire();

                    if (NetworkServer.active) attackerBody.SetBuffCount(FFTSContent.Buffs.bdMoonshineStack.buffIndex, 0);

                    moonshineComponent.savedDamage = 0f;
                }
            }

            orig.Invoke(self, damageInfo);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<EquipmentAssetCollection>("acMoonshineFlask", FFTSBundle.Equipments);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            Component.Destroy(body.gameObject.GetComponent<MoonshineBehaviour>());
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.gameObject.AddComponent<MoonshineBehaviour>();
        }

        public class MoonshineBehaviour : MonoBehaviour
        {
            public float savedDamage = 0f;
        }
    }
}