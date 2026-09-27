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
/*using ThreeEyedGames;
using EmotesAPI;*/
using RoR2.Skills;
using FortunesFromTheScrapyard;
using MSU.Config;

namespace FortunesFromTheScrapyard.Monsters.Gardener
{
    public class GardenerMonster : FFTSMonster
    {

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

        public override FFTSAssetRequest<MonsterAssetCollection> LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<MonsterAssetCollection>("acGardener", FFTSBundle.Monsters);
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }

        private void Hooks()
        {
            if (FFTSMain.emotesInstalled)
            {
                //Emotes();
            }
        }

        /*private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = FFTSAssets.GetAssetBundle(FFTSBundle.Indev).LoadAsset<GameObject>("wrecker_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "wrecker_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "wrecker_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: true);
                }
            }
        }*/

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
    }
}