using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.UI;
using FortunesFromTheScrapyard.Survivors.Skater.Components;
using EntityStates;
//using FortunesFromTheScrapyard.Survivors.Skater.EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;
using System.Runtime.CompilerServices;
using ThreeEyedGames;
using EmotesAPI;
using RoR2.Skills;
using MSU.Config;
using EntityStates.Skater;

namespace FortunesFromTheScrapyard.Survivors.Skater
{
    public class SkaterSurvivor : ScrapyardSurvivor
    {
        //Values
        public const string PRIMARYTOKEN = "SCRAPYARD_Skater_SALVO_DESC";
        public const string SECONDARY = "SCRAPYARD_Skater_KINETIC_DESC";
        public const string SPECIALTOKEN = "SCRAPYARD_Skater_CLONE_DESC";

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePrimaryDamageCoefficient = 0.75f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SECONDARY, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        internal static float bubbleDamageCoefficient = 1.05f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SPECIALTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseCloneDamageCoefficient = 5f;

        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType SkaterBubble;
        public static DamageAPI.ModdedDamageType SkaterBubbleMask;

        //Effects
        internal static GameObject skaterMuzzleFlash;

        internal static GameObject skaterTempExplosion;

        internal static GameObject bubbledEffect;

        //Temp
        internal static Material tempBubbledMat;

        //Models
        //Projectiles
        internal static GameObject bubbleProjectile;
        //Sounds
        //Color
        internal static Color skaterColor = Color.cyan;
        internal static Color skaterSecondaryColor = Color.blue;

        //UI

        public override void Initialize()
        {
            Default = DamageAPI.ReserveDamageType();
            SkaterBubble = DamageAPI.ReserveDamageType();
            SkaterBubbleMask = DamageAPI.ReserveDamageType();

            CreateEffects();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            CreateUI();

            ModifyPrefab();

            Hooks();
        }

        public static int DeployableSlotLimitDelegate(CharacterMaster master, int multiplier)
        {
            return 4;
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
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acSkater", ScrapyardBundle.Indev);
        }
        #region effects
        private void CreateEffects()
        {
            tempBubbledMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matIsFrozen.mat").WaitForCompletion();

            skaterMuzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SoulSpiralMuzzleflashVFX.prefab").WaitForCompletion().InstantiateClone("SkaterMuzzleFlash");

            Component.Destroy(skaterMuzzleFlash.transform.Find("Flash").gameObject.GetComponent<ParticleSystem>());

            skaterMuzzleFlash.transform.Find("Flash").Find("Petals, Burst").localScale = new Vector3(0.1f, 0.1f, 0.1f);
            skaterMuzzleFlash.transform.Find("Flash").Find("Dissapate, Swipes").localScale = new Vector3(0.1f, 0.1f, 0.1f);
            skaterMuzzleFlash.transform.Find("Flash").Find("HighCourtspark").localScale = new Vector3(0.1f, 0.1f, 0.1f);
            skaterMuzzleFlash.transform.Find("Point Light").position = Vector3.zero;
            skaterMuzzleFlash.transform.Find("Point Light").gameObject.GetComponent<Light>().range = 4;

            ScrapyardContent.CreateAndAddEffectDef(skaterMuzzleFlash);

            skaterTempExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombExplosion.prefab").WaitForCompletion();

            if (!skaterTempExplosion.GetComponent<EffectComponent>()) skaterTempExplosion.AddComponent<EffectComponent>();

            skaterTempExplosion.GetComponent<EffectComponent>().applyScale = true;

            ScrapyardContent.CreateAndAddEffectDef(skaterTempExplosion);

            bubbledEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OutOfCombatArmor/OutOfCombatArmorEffect.prefab").WaitForCompletion().InstantiateClone("UnforgivenShieldReadyEffect", false);
            bubbledEffect.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", skaterColor);
            bubbledEffect.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[1].SetColor("_TintColor", skaterColor);
            var shieldMain = bubbledEffect.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().main;
            shieldMain.startColor = skaterColor;
            bubbledEffect.transform.GetChild(0).Find("Trigger").Find("Sphere, Quick").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", skaterColor);
            bubbledEffect.transform.GetChild(0).Find("Trigger").Find("Sphere").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", skaterColor);
            bubbledEffect.transform.GetChild(0).Find("Trigger").Find("Point Light").GetComponent<Light>().color = skaterColor;
        }

        #endregion
        #region projectiles
        private void CreateProjectiles()
        {
            bubbleProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/Fireball.prefab").WaitForCompletion().InstantiateClone("SkaterBubbleProjectile");
            if (!bubbleProjectile.GetComponent<NetworkIdentity>()) bubbleProjectile.AddComponent<NetworkIdentity>();

            bubbleProjectile.transform.localScale = new Vector3(1, 1, 1);

            ProjectileController bubbleController = bubbleProjectile.GetComponent<ProjectileController>();
            bubbleController.ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBombGhost.prefab").WaitForCompletion();

            ProjectileSimple bubbleSimple = bubbleProjectile.GetComponent<ProjectileSimple>();
            bubbleSimple.lifetime = 3.5f;
            bubbleSimple.enabled = true;
            bubbleSimple.desiredForwardSpeed = 90f;
            bubbleSimple.enableVelocityOverLifetime = true;
            bubbleSimple.velocityOverLifetime = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(0.25f, 0.1f), new Keyframe(1f, 0f) });

            ProjectileSteerTowardTarget bubbleSteer = bubbleProjectile.AddComponent<ProjectileSteerTowardTarget>();
            bubbleSteer.yAxisOnly = false;
            bubbleSteer.rotationSpeed = 700f;

            ProjectileDirectionalTargetFinder bubbletFinder = bubbleProjectile.AddComponent<ProjectileDirectionalTargetFinder>();
            bubbletFinder.lookRange = 15f;
            bubbletFinder.lookCone = 360f;
            bubbletFinder.targetSearchInterval = 0.2f;
            bubbletFinder.onlySearchIfNoTarget = true;
            bubbletFinder.allowTargetLoss = true;
            bubbletFinder.testLoS = true;
            bubbletFinder.ignoreAir = false;
            bubbletFinder.flierAltitudeTolerance = Mathf.Infinity;

            DamageAPI.ModdedDamageTypeHolderComponent mdthc = bubbleProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            mdthc.Add(SkaterBubble);

            ProjectileSingleTargetImpact projectileSingleTargetImpact = bubbleProjectile.GetComponent<ProjectileSingleTargetImpact>();
            projectileSingleTargetImpact.impactEffect = skaterTempExplosion;

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(bubbleProjectile);
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
            bool buffRequired(CharacterBody body) => body.HasBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff);
            float radius(CharacterBody body) => body.radius;
            TempVisualEffectAPI.AddTemporaryVisualEffect(bubbledEffect, radius, buffRequired);

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

            if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
            }
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            CharacterBody victimBody = damageReport.victimBody;
            GameObject victim = damageReport.victim.gameObject;

            if (victimBody)
            {
                if (damageInfo.HasModdedDamageType(SkaterBubble))
                {
                    if (!victimBody.isBoss)
                    {
                        CharacterMotor victimMotor = victim.GetComponent<CharacterMotor>();
                        victim.GetComponent<RigidbodyMotor>();
                        if (victimMotor && victimMotor.isGrounded && !victimBody.isChampion && (victimBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == 0 && !victimBody.HasBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff))
                        {
                            victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff, 2.5f);
                            if (!victimBody.mainHurtBox)
                            {
                                _ = victimBody.transform;
                            }
                            else
                            {
                                _ = victimBody.mainHurtBox.transform;
                            }
                            Vector3 upVector = new Vector3(0f, 1f, 0f);
                            float massCalc = victimMotor.mass * 20f;
                            float finalCalc = massCalc + massCalc / 10f * 2f;
                            victimMotor.ApplyForce(finalCalc * upVector);

                            if (victim.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                            {
                                BubbledState bubbledState = new BubbledState();
                                setStateOnHurt.targetStateMachine.SetInterruptState(bubbledState, InterruptPriority.Frozen);

                                EntityStateMachine[] array = setStateOnHurt.idleStateMachine;

                                for (int i = 0; i < array.Length; i++)
                                {
                                    array[i].SetNextState(new Idle());
                                }
                            }
                            else if (victim.TryGetComponent(out EntityStateMachine entityMachine))
                            {
                                BubbledState bubbledState = new BubbledState();
                                entityMachine.SetInterruptState(bubbledState, InterruptPriority.Frozen);
                            }
                        }
                        else
                        {
                            victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff, 4f);
                        }
                    }
                }
            }
        }

        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("SkaterBody"))
            {
                foreach (LanguageTextMeshController i in self.gameObject.GetComponentsInChildren<LanguageTextMeshController>())
                {
                    if (i && i.token == "LOADOUT_SKILL_MISC") i.token = "Passive";
                }
            }
        }
        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                CharacterBody attackerBody = null;

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                }

                if (damageInfo.damage > 0 && !damageInfo.rejected && victimBody && attackerBody)
                {
                    if (victimBody.HasBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff) && !damageInfo.HasModdedDamageType(SkaterBubbleMask))
                    {
                        damageInfo.rejected = true;

                        BlastAttack blastAttack = new BlastAttack();

                        blastAttack.procCoefficient = damageInfo.procCoefficient;
                        blastAttack.attacker = damageInfo.attacker;
                        blastAttack.inflictor = damageInfo.inflictor;
                        blastAttack.teamIndex = attackerBody.teamComponent.teamIndex;
                        blastAttack.baseDamage = damageInfo.damage;
                        blastAttack.baseForce = 200f;
                        blastAttack.position = damageInfo.position;
                        blastAttack.radius = 8f;
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        blastAttack.damageType = DamageType.Generic;
                        blastAttack.damageColorIndex = DamageColorIndex.Void;
                        blastAttack.crit = damageInfo.crit;
                        blastAttack.AddModdedDamageType(SkaterBubbleMask);
                        blastAttack.Fire();

                        EffectManager.SpawnEffect(skaterTempExplosion, new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Quaternion.identity,
                            scale = 5f
                        }, true);
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Indev).LoadAsset<GameObject>("skater_emoteskeleton");
                CustomEmotesAPI.ImportArmature(this.characterPrefab, skele);
            };
            CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
        }
        private void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
        {
            if (newAnimation != "none")
            {
                if (mapper.transform.name == "skater_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshSmg.L").gameObject.SetActive(value: false);
                    mapper.transform.parent.Find("meshSmg.R").gameObject.SetActive(value: false);
                    mapper.transform.parent.Find("meshSniper").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "skater_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshSmg.L").gameObject.SetActive(value: true);
                    mapper.transform.parent.Find("meshSmg.R").gameObject.SetActive(value: true);
                    mapper.transform.parent.Find("meshSniper").gameObject.SetActive(value: true);
                }
            }
        }
    }
}

