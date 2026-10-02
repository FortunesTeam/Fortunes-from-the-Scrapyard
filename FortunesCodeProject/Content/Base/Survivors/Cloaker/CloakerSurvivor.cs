using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class Cloaker : FFTSSurvivor
    {
        public static DamageAPI.ModdedDamageType CloakerChargedDamageType;
        public static DamageAPI.ModdedDamageType CloakerScreechDamageType;

        public override void Initialize()
        {
            CloakerChargedDamageType = DamageAPI.ReserveDamageType();
            CloakerScreechDamageType = DamageAPI.ReserveDamageType();
            CloakerAssets.Initialize(assetCollection);
            ModifyPrefab();
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
            if (body.hasCloakBuff) args.damageMultAdd += 1.5f;
            if (controller.isAkimbo) args.attackSpeedMultAdd += 1.5f;
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
                self.body.AddTimedBuff(FFTSContent.Buffs.bdCloakerMarkCd, 5f);
                if (!info.crit) info.crit = true;
                else info.damage *= 1.5f;
                EffectManager.SimpleImpactEffect(CloakerAssets.ConsumeEffect, info.position, Vector3.up, true);
            }
            orig(self, info);
        }
    }
}
