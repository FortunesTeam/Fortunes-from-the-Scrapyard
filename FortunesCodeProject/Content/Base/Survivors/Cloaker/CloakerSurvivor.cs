using System.Collections;
using System.Linq;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class Cloaker : FFTSSurvivor
    {
        public const string PASSIVETOKEN = "FFTS_CLOAKER_PASSIVE_DESCRIPTION";
        public const string AKIMBOTOKEN = "FFTS_CLOAKER_PASSIVE_ALT1_DESCRIPTION";
        public const string PRIMARYTOKEN = "FFTS_CLOAKER_PRIMARY_DESCRIPTION";
        public const string SECONDARYTOKEN = "FFTS_CLOAKER_SECONDARY_DESCRIPTION";
        public const string UTILITYTOKEN = "FFTS_CLOAKER_UTILITY_DESCRIPTION";
        public const string MARKTOKEN = "FFTS_CLOAKER_SPECIAL_DESCRIPTION";
        public const string SCREECHTOKEN = "FFTS_CLOAKER_SPECIAL2_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Additive damage bonus while cloaked. 1.5 grants 150% bonus damage.")]
        [FormatToken(PASSIVETOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float CloakDamageBonus = 1.5f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Additive attack speed bonus with Akimbo. 1.5 grants 150% bonus attack speed.")]
        [FormatToken(AKIMBOTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float AkimboAttackSpeedBonus = 1.5f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Seconds before a consumed mark can be reapplied to the same target.")]
        [FormatToken(MARKTOKEN, 2)]
        public static float MarkCooldown = 5f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Additional damage when a marked hit already crits. 0.5 grants 50% more damage.")]
        [FormatToken(MARKTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float MarkCritDamageBonus = 0.5f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Shoot Harder's recharge time in seconds. Requires a restart.")]
        public static float SecondaryCooldown = 4f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Restealth's recharge time in seconds. Requires a restart.")]
        public static float UtilityCooldown = 7f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Screech's recharge time in seconds. Requires a restart.")]
        public static float ScreechCooldown = 15f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Shoot Harder's maximum stock. Requires a restart.")]
        public static int SecondaryStock = 1;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Restealth's maximum stock. Requires a restart.")]
        public static int UtilityStock = 1;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Screech's maximum stock. Requires a restart.")]
        public static int ScreechStock = 1;

        public static DamageAPI.ModdedDamageType CloakerChargedDamageType;
        public static DamageAPI.ModdedDamageType CloakerScreechDamageType;

        public override void Initialize()
        {
            CloakerChargedDamageType = DamageAPI.ReserveDamageType();
            CloakerScreechDamageType = DamageAPI.ReserveDamageType();
            CloakerAssets.Initialize(assetCollection);
            ModifyPrefab();
            FFTSMain.instance.StartCoroutine(ConfigureSkills());
            On.RoR2.SurvivorCatalog.Init += SurvivorCatalog_Init;
            On.RoR2.HealthComponent.TakeDamageProcess += TakeDamage;
            GlobalEventManager.onServerDamageDealt += DamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
        }

        public void ModifyPrefab()
        {
            characterPrefab.GetComponent<CharacterBody>().preferredPodPrefab =
                CloakerAssets.LoadAddress<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab");
            characterPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab =
                CloakerAssets.LoadAddress<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab");
            CloakerAssets.ConfigureAudio(characterPrefab);
        }

        private IEnumerator ConfigureSkills()
        {
            while (!ConfigSystem.configsBound) yield return null;

            SkillLocator skills = characterPrefab.GetComponent<SkillLocator>();
            SkillDef secondary = skills.secondary.skillFamily.variants[0].skillDef;
            SkillDef utility = skills.utility.skillFamily.variants[0].skillDef;
            SkillDef screech = skills.special.skillFamily.variants.Single(
                variant => variant.skillDef.skillDescriptionToken == SCREECHTOKEN).skillDef;
            secondary.baseRechargeInterval = SecondaryCooldown;
            secondary.baseMaxStock = SecondaryStock;
            utility.baseRechargeInterval = UtilityCooldown;
            utility.baseMaxStock = UtilityStock;
            screech.baseRechargeInterval = ScreechCooldown;
            screech.baseMaxStock = ScreechStock;
        }

        public override bool IsAvailable(ContentPack contentPack) => true;

        public override FFTSAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
            => FFTSAssets.LoadAssetAsync<SurvivorAssetCollection>("acCloaker", FFTSBundle.Survivors);

        private void SurvivorCatalog_Init(On.RoR2.SurvivorCatalog.orig_Init orig)
        {
            orig();
            if (FFTSMain.emotesInstalled)
                CloakerEmotes.Initialize(characterPrefab, assetCollection.FindAsset<GameObject>("cloaker_emoteskeleton"));
        }

        private void RecalculateStats(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!body.TryGetComponent(out CloakerController controller)) return;
            if (body.hasCloakBuff) args.damageMultAdd += CloakDamageBonus;
            if (controller.isAkimbo) args.attackSpeedMultAdd += AkimboAttackSpeedBonus;
        }

        private void DamageDealt(DamageReport report)
        {
            if (!NetworkServer.active || !report.victimBody || !report.attackerBody) return;
            CharacterBody victim = report.victimBody;
            if (victim.hasCloakBuff && victim.TryGetComponent(out CloakerController controller) && controller.graceTimer <= 0f)
                controller.BreakStealth();
        }

        private void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo info)
        {
            if (NetworkServer.active && self.body && info.attacker && info.attacker.GetComponent<CharacterBody>()
                && self.body.HasBuff(FFTSContent.Buffs.bdCloakerMarked))
            {
                self.body.RemoveBuff(FFTSContent.Buffs.bdCloakerMarked);
                self.body.AddTimedBuff(FFTSContent.Buffs.bdCloakerMarkCd, MarkCooldown);
                if (!info.crit) info.crit = true;
                else info.damage *= 1f + MarkCritDamageBonus;
                EffectManager.SimpleImpactEffect(CloakerAssets.ConsumeEffect, info.position, Vector3.up, true);
            }
            orig(self, info);
        }
    }
}
