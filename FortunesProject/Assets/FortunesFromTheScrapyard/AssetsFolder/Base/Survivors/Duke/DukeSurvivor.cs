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
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using EntityStates;
//using FortunesFromTheScrapyard.Survivors.Duke.EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using EmotesAPI;
using RoR2.Skills;
using MSU.Config;
using R2API.Networking.Interfaces;

namespace FortunesFromTheScrapyard.Survivors.Duke
{
    public class DukeSurvivor : ScrapyardSurvivor
    {
        //Values
        public const string SALVOTOKEN = "SCRAPYARD_DUKE_SALVO_DESC";
        public const string MINETOKEN = "SCRAPYARD_DUKE_KINETIC_DESC";
        public const string CLONETOKEN = "SCRAPYARD_DUKE_CLONE_DESC";

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SALVOTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseSalvoDamageCoefficient = 3.5f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(MINETOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        internal static float damageShareCoefficient = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(CLONETOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseCloneDamageCoefficient = 5f;

        public static DamageAPI.ModdedDamageType DukeFourthShot;

        //ALL TEMP
        internal static GameObject dukeTracer;
        internal static GameObject dukeTracerCrit;

        internal static GameObject dukeBoomEffect;

        internal static GameObject bullet;
        internal static GameObject gun;
        internal static GameObject casing;

        internal static GameObject dukePistolSpinEffect;

        //Projectile
        internal static GameObject damageShareMine;
        internal static GameObject dukeField;
        //Sounds
        //Color
        internal static Color orange = new Color(255f / 255f, 127f / 255f, 80f / 255f);

        //UI
        internal static Sprite primaryIcon;
        internal static Sprite primaryEmpoweredIcon;

        public override void Initialize()
        {
            DukeFourthShot = DamageAPI.ReserveDamageType();

            CreateEffects();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            CreateUI();

            ModifyPrefab();

            Hooks();
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
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acDuke", ScrapyardBundle.Survivors);
        }
        #region effects
        private static void CreateEffects()
        {
            dukeTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunLight.prefab").WaitForCompletion();

            dukeTracerCrit = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();

            EffectDef dukeTracerEffectDef = new EffectDef(dukeTracer);
            EffectDef dukeTracerCritEffectDef = new EffectDef(dukeTracerCrit);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(dukeTracerEffectDef);
            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(dukeTracerCritEffectDef);

            dukeBoomEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainAirstrikeImpact1.prefab").WaitForCompletion().InstantiateClone("CadetBoomProjectile");
            dukeBoomEffect.GetComponent<EffectComponent>().applyScale = true;
            dukeBoomEffect.GetComponent<EffectComponent>().soundName = "Play_captain_shift_impact";

            EffectDef dukeBoomEffectDef = new EffectDef(dukeBoomEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(dukeBoomEffectDef);

            dukePistolSpinEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoReloadFX.prefab").WaitForCompletion().InstantiateClone("DukePistolSpinEffect");
        }

        #endregion
        #region projectiles
        private void CreateProjectiles()
        {
            damageShareMine = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAlt.prefab").WaitForCompletion().InstantiateClone("DukeDamageShareMine");
            if(!damageShareMine.GetComponent<NetworkIdentity>()) damageShareMine.AddComponent<NetworkIdentity>();

            damageShareMine.GetComponent<ProjectileController>().ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineGhostReskinColossus.prefab").WaitForCompletion().InstantiateClone("DukeMineGhost");

            ProjectileStickOnImpact sticky = damageShareMine.GetComponent<ProjectileStickOnImpact>();
            sticky.ignoreCharacters = false;

            ProjectileExplosion pe = damageShareMine.GetComponent<ProjectileExplosion>();

            ProjectileSimple ps = damageShareMine.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 200f;

            ProjectileFuse projectileFuse = damageShareMine.GetComponent<ProjectileFuse>();
            projectileFuse.fuse = 0f;

            dukeField = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("DukeDamageField");
            if (!dukeField.GetComponent<NetworkIdentity>()) dukeField.AddComponent<NetworkIdentity>();

            Material[] iHateMaterialSetup = new Material[2];
            iHateMaterialSetup[0] = dukeField.transform.Find("AreaIndicator").Find("Sphere").gameObject.GetComponent<MeshRenderer>().sharedMaterials[1];
            iHateMaterialSetup[1] = assetCollection.FindAsset<Material>("matDukeDome");
            dukeField.transform.Find("AreaIndicator").Find("Sphere").gameObject.GetComponent<MeshRenderer>().materials = iHateMaterialSetup;
            dukeField.transform.Find("AreaIndicator").Find("ChargeIn").gameObject.GetComponent<ParticleSystemRenderer>().material = assetCollection.FindAsset<Material>("matDukeChargeIn");
            dukeField.transform.Find("AreaIndicator").Find("Core").gameObject.GetComponent<ParticleSystemRenderer>().material = assetCollection.FindAsset<Material>("matDukeFieldSphere");
            dukeField.transform.Find("AreaIndicator").Find("Point Light").gameObject.GetComponent<Light>().color = orange;
            var fieldMain = dukeField.transform.Find("AreaIndicator").Find("SoftGlow").gameObject.GetComponent<ParticleSystem>().main;
            fieldMain.startColor = orange;

            BuffWard buffWard = dukeField.GetComponent<BuffWard>();
            buffWard.invertTeamFilter = true;
            buffWard.buffDef = ScrapyardContent.Buffs.bdDukeDamageShare;
            buffWard.interval = 1f;
            buffWard.expireDuration = 6f;
            buffWard.radius = 15f;

            dukeField.GetComponent<SphereCollider>().radius = 15f;

            UnityEngine.Object.Destroy(dukeField.GetComponent<SlowDownProjectiles>());

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(dukeField);

            pe.childrenProjectilePrefab = dukeField;

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(damageShareMine);
        }
        #endregion

        #region sounds
        #endregion

        #region UI
        private void CreateUI()
        {
            primaryIcon = assetCollection.FindAsset<Sprite>("iconDukePrimary");
            primaryEmpoweredIcon = assetCollection.FindAsset<Sprite>("iconDukePrimaryEmpower");
        }
        #endregion
        private void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare))
            {
                args.moveSpeedReductionMultAdd += 0.25f;
            }
            if (sender.HasBuff(ScrapyardContent.Buffs.bdDukeSpeedBuff))
            {
                args.moveSpeedMultAdd += 0.25f * sender.GetBuffCount(ScrapyardContent.Buffs.bdDukeSpeedBuff);
            }

            if (sender.bodyIndex == BodyCatalog.FindBodyIndex("DukeBody"))
            {
                DukeController dukeController = sender.GetComponent<DukeController>();
                if (dukeController != null)
                {
                    float baseAttackSpeed = sender.baseAttackSpeed + (sender.levelAttackSpeed * sender.level);
                    //float baseDamage = sender.baseDamage + (sender.baseDamage * sender.level);
                    //float newDamage = sender.damage * ((sender.attackSpeed - baseAttackSpeed) * 0.7f);
                    //sender.damage += newDamage;
                    dukeController.attackSpeedConversion = (sender.attackSpeed - baseAttackSpeed) * 0.7f;
                    args.attackSpeedReductionMultAdd += (sender.attackSpeed - baseAttackSpeed) * 0.7f;
                }
            }

            if (sender.bodyIndex == BodyCatalog.FindBodyIndex("DukeDecoyBody"))
            {
                float baseAttackSpeed = sender.baseAttackSpeed + (sender.levelAttackSpeed * sender.level);
                args.attackSpeedReductionMultAdd += (sender.attackSpeed - baseAttackSpeed) * 0.7f;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Survivors).LoadAsset<GameObject>("duke_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "duke_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshDukeGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "duke_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshDukeGun").gameObject.SetActive(value: true);
                }
            }
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            GameObject attackerObject = damageReport.attacker.gameObject;
            DukeController dukeController = attackerBody.GetComponent<DukeController>();
            if (NetworkServer.active)
            {
                if (attackerBody && victimBody)
                {
                    if (damageInfo.HasModdedDamageType(DukeFourthShot))
                    {
                        int amount = 5;
                        float harpoon = 2.5f;
                        attackerBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeSpeedBuff);
                        for (int l = 0; l < amount; l++)
                        {
                            attackerBody.AddTimedBuff(ScrapyardContent.Buffs.bdDukeSpeedBuff, harpoon * (float)(l + 1) / (float)amount);
                        }
                        EffectData effectData = new EffectData();
                        effectData.origin = attackerBody.corePosition;
                        CharacterMotor characterMotor = attackerBody.characterMotor;
                        bool flag = false;
                        if ((bool)characterMotor)
                        {
                            Vector3 moveDirection = characterMotor.moveDirection;
                            if (moveDirection != Vector3.zero)
                            {
                                effectData.rotation = Util.QuaternionSafeLookRotation(moveDirection);
                                flag = true;
                            }
                        }

                        if (!flag)
                        {
                            effectData.rotation = attackerBody.transform.rotation;
                        }

                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MoveSpeedOnKillActivate"), effectData, transmit: true);

                        if(attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("DukeBody") && attackerBody.skillLocator.utility)
                        {
                            if (attackerBody.skillLocator.utility.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Flourish")))
                            {
                                NetworkIdentity networkIdentity = attackerBody.networkIdentity;

                                new SyncDukeRecharge(networkIdentity.netId).Send(R2API.Networking.NetworkDestination.Clients);
                            }
                        }
                    }
                }
            }
        }
    }
}

