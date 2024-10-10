using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.UI;
using FortunesFromTheScrapyard.Survivors.Maiden.Components;
using EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using EmotesAPI;
using RoR2.Skills;
using MSU.Config;
using R2API.Networking.Interfaces;
using FortunesFromTheScrapyard.Ricochet;

namespace FortunesFromTheScrapyard.Survivors.Maiden
{
    public class MaidenSurvivor : ScrapyardSurvivor
    {
        public static DeployableSlot MaidenDiceSlot;

        //ALL TEMP

        //Projectile
        internal static GameObject DiceProjectile;
        //Sounds
        //Color
        internal static Color ourple = Color.Lerp(Color.red, Color.cyan, 0.5f);

        //UI

        public override void Initialize()
        {
            MaidenDiceSlot = DeployableAPI.RegisterDeployableSlot(DeployableSlotLimitDelegate);

            CreateEffects();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            CreateUI();

            ModifyPrefab();

            Hooks();
        }

        public static int DeployableSlotLimitDelegate(CharacterMaster master, int multiplier)
        {
            return 6;
        }
        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/Bandit2Crosshair");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acMaiden", ScrapyardBundle.Indev);
        }
        #region effects
        private void CreateEffects()
        {
        }

        #endregion
        #region projectiles
        private void CreateProjectiles()
        {
            DiceProjectile = assetCollection.FindAsset<GameObject>("DiceProjectile");
            DiceProjectile.GetComponent<ProjectileDeployToOwner>().deployableSlot = MaidenDiceSlot;
        }
        #endregion

        #region sounds
        #endregion

        #region UI
        private void CreateUI()
        {
        }
        #endregion
        private void Hooks()
        {
            

            if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
            }
        }
        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Survivors).LoadAsset<GameObject>("maiden_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "maiden_emoteskeleton")
                {
                    //mapper.transform.parent.Find("meshDukeGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "maiden_emoteskeleton")
                {
                    //mapper.transform.parent.Find("meshDukeGun").gameObject.SetActive(value: true);
                }
            }
        }
    }
}

