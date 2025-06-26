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

namespace FortunesFromTheScrapyard.Monsters.Gump
{
    public class GumpMonster : FFTSMonster
    {
        public const string PRIMARYTOKEN = "SCRAPYARD_GUMP_PRIMARY_DESC";
        public const string SECONDARYTOKEN = "SCRAPYARD_GUMP_SECONDARY_DESC";
        public const string UTILITYTOKEN = "SCRAPYARD_GUMP_UTILITY_DESC";
        public const string SPECIALTOKEN = "SCRAPYARD_GUMP_SPECIAL_DESC";

        [ConfigureField(FFTSConfig.ID_MONSTERS)]
        [FormatToken(PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePrimaryDamage = 0.6f;

        [ConfigureField(FFTSConfig.ID_MONSTERS)]
        [FormatToken(SECONDARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        internal static float baseSecondaryDamage = 1.2f;

        [ConfigureField(FFTSConfig.ID_MONSTERS)]
        [FormatToken(UTILITYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseUtilityDamage = 4f;

        [ConfigureField(FFTSConfig.ID_MONSTERS)]
        [FormatToken(SPECIALTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseSpecialDamage = 6f;

        // DamageTypes
        public static DamageAPI.ModdedDamageType GumpExplode;


        //Projectiles
        internal static GameObject diskPrefab;
        internal static GameObject diskGhost;
        internal static GameObject diskExplosion;

        internal static GameObject soundScape;
        internal static GameObject soundWave;

        public override void Initialize()
        {
            //BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            //On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            //GumpExplode = DamageAPI.ReserveDamageType();

            ModifyPrefab();

            Hooks();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override FFTSAssetRequest<MonsterAssetCollection> LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<MonsterAssetCollection>("acGump", FFTSBundle.Indev);
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