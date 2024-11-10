using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.UI;
using EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using EmotesAPI;
using RoR2.Skills;
using FortunesFromTheScrapyard;
using MSU.Config;

namespace FortunesFromTheScrapyard.Monsters.Gardener
{
    public class GardenerMonster : ScrapyardMonster
    {
        // lol I have no CLUE what I'm doing, If ANY of this looks right it by the grace of God or Kenko - EZ


        public const string PRIMARYTOKEN = "SCRAPYARD_GARDENER_PRIMARY_DESC";
        public const string SECONDARYTOKEN = "SCRAPYARD_GARDENER_SECONDARY_DESC";
        public const string UTILITYTOKEN = "SCRAPYARD_GARDENER_UTILITY_DESC";
        public const string SPECIALTOKEN = "SCRAPYARD_GARDENER_SPECIAL_DESC";

        [ConfigureField(ScrapyardConfig.ID_MONSTERS)]
        [FormatToken(PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePrimaryDamage = 0.6f;

        [ConfigureField(ScrapyardConfig.ID_MONSTERS)]
        [FormatToken(SECONDARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        internal static float baseSecondaryDamage = 1.2f;

        [ConfigureField(ScrapyardConfig.ID_MONSTERS)]
        [FormatToken(UTILITYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseUtilityDamage = 4f;

        [ConfigureField(ScrapyardConfig.ID_MONSTERS)]
        [FormatToken(SPECIALTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseSpecialDamage = 6f;

        // DamageTypes
        public static DamageAPI.ModdedDamageType GardenerExplode;


        //Projectiles
        internal static GameObject wispProjectilePrefab;


        public override void Initialize()
        { 
            //BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            //On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            //GardenerExplode = DamageAPI.ReserveDamageType();

            ModifyPrefab();

            CreateProjectiles();

            Hooks();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<MonsterAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<MonsterAssetCollection>("acGardener", ScrapyardBundle.Indev);
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        private void Hooks()
        {
            //GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;

            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamageProcess;

            if (ScrapyardMain.emotesInstalled)
            {
                //Emotes();
            }
        }

        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Indev).LoadAsset<GameObject>("badger_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "badger_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "badger_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: true);
                }
            }
        }

        #region projectiles
        private void CreateProjectiles()
        {
            wispProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Gravekeeper/GravekeeperTrackingFireball.prefab").WaitForCompletion().InstantiateClone("GardenerTrackingFireball");

            ProjectileSteerTowardTarget projectileSteerTowardTarget = wispProjectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
            projectileSteerTowardTarget.rotationSpeed = 195f;

            ProjectileDirectionalTargetFinder projectileDirectionalTargetFinder = wispProjectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
            projectileDirectionalTargetFinder.lookRange = 130f;

            VelocityRandomOnStart velocityRandomOnStart = wispProjectilePrefab.GetComponent<VelocityRandomOnStart>();
            velocityRandomOnStart.minSpeed = 20f;
            velocityRandomOnStart.maxSpeed = 25f;
            velocityRandomOnStart.directionMode = VelocityRandomOnStart.DirectionMode.Cone;
            //velocityRandomOnStart.coneAngle = 30f;



        }
        #endregion

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
        }

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            
        }  
    }
}