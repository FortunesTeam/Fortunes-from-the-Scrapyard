using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class Cloaker : ScrapyardSurvivor
    {
        public static GameObject cloakerConsumeEffect;

        public static GameObject CloakerRangeIndicatorPrefab;

        public static DamageAPI.ModdedDamageType CloakerChargedDamageType;
        public static DamageAPI.ModdedDamageType CloakerAkimboDamageType;
        public override void Initialize()
        {
            CloakerChargedDamageType = DamageAPI.ReserveDamageType();
            CloakerAkimboDamageType = DamageAPI.ReserveDamageType();

            bool tempAdd(CharacterBody body) => body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked);

            CreateEffects();

            Hooks();

            TempVisualEffectAPI.AddTemporaryVisualEffect(assetCollection.FindAsset<GameObject>("CloakerMarkEffect"), tempAdd, true);

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acCloaker", ScrapyardBundle.Indev);
        }
        private void CreateEffects()
        {
            cloakerConsumeEffect = assetCollection.FindAsset<GameObject>("CloakerMarkedConsumeEffect");

            ScrapyardContent.CreateAndAddEffectDef(cloakerConsumeEffect);

            GameObject cloakerRange = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion();

            CloakerRangeIndicatorPrefab = assetCollection.FindAsset<GameObject>("CloakerRangeIndicator");
            CloakerRangeIndicatorPrefab.EnsureComponent<NetworkIdentity>();

            CloakerRangeIndicatorPrefab.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material = cloakerRange.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material;
            CloakerRangeIndicatorPrefab.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.2358491f, 0.1768868f, 0.2268582f));

            CloakerRangeIndicatorPrefab.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material = cloakerRange.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material;
            CloakerRangeIndicatorPrefab.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.2358491f, 0.1768868f, 0.2268582f));
        }
        #region projectiles
        private void CreateProjectiles()
        {
            
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
        }
        #endregion

        #region UI
        private static void CreateUI()
        {
        }
        #endregion
        private void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.bodyIndex == BodyCatalog.FindBodyIndex("CloakerBody"))
            {
                if (sender.hasCloakBuff) args.damageMultAdd += 1.5f;

                if (sender.TryGetComponent<CloakerController>(out var cloakerController)) cloakerController.SetStealthCooldown();
            }

            if (sender.TryGetComponent<CloakerController>(out var cloak2) && cloak2.isAkimbo)
            {
                args.attackSpeedMultAdd += 1.5f;
            }
        }
        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;

            CharacterBody victimBody = damageReport.victimBody;
            CharacterBody attackerBody = damageReport.attackerBody;

            if(victimBody && attackerBody)
            {
                if (damageReport.damageInfo.HasModdedDamageType(CloakerAkimboDamageType) && !victimBody.HasBuff(ScrapyardContent.Buffs.bdCloakerMarkCd))
                {
                    victimBody.AddBuff(ScrapyardContent.Buffs.bdCloakerMarked);
                }

                if(victimBody.bodyIndex == BodyCatalog.FindBodyIndex("CloakerBody") && victimBody.hasCloakBuff)
                {
                    if (victimBody.TryGetComponent<CloakerController>(out var cloakerController) && cloakerController.graceTimer <= 0f)
                    {
                        victimBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                        victimBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                        cloakerController.passiveCloakOn = false;
                    }
                }
            }
        }

        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody victimBody = self.body;
            damageInfo.attacker.TryGetComponent<CharacterBody>(out var attackerBody);

            if (attackerBody && victimBody)
            {
                if (victimBody.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked))
                {
                    if (NetworkServer.active)
                    {
                        victimBody.RemoveBuff(ScrapyardContent.Buffs.bdCloakerMarked);
                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdCloakerMarkCd, 5f);
                    }

                    if (!damageInfo.crit) damageInfo.crit = true;
                    else damageInfo.damage *= 1.5f;

                    EffectManager.SimpleImpactEffect(cloakerConsumeEffect, damageInfo.position, Vector3.up, transmit: true);
                }
            }

            orig.Invoke(self, damageInfo);
        }
    }
}

