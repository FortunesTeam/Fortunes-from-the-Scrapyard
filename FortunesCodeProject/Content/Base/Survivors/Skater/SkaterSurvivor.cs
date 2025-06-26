using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Skater
{
    public class Skater : FFTSSurvivor
    {
        public override void Initialize()
        {
            CreateEffects();

            Hooks();

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override FFTSAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<SurvivorAssetCollection>("acSkater", FFTSBundle.Survivors);
        }
        private void CreateEffects()
        {
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(FFTSContent.Buffs.bdSkaterSpeedBuff);
            if (buffCount > 0)
            {
                args.baseMoveSpeedAdd += buffCount * 0.015f;
            }
        }
    }
}

