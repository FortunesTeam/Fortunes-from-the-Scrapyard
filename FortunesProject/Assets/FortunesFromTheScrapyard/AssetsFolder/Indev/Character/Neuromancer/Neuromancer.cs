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
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using EntityStates;
using FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates;
using RoR2.Projectile;
using RoR2.EntityLogic;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer
{
    public class Neuromancer : ScrapyardSurvivor
    {
        public static DamageAPI.ModdedDamageType DelayedPrimary;
        public static DamageAPI.ModdedDamageType DelayedSecondary;
        public static DamageAPI.ModdedDamageType DelayedUtility;
        public static DamageAPI.ModdedDamageType DelayedPunch;
        public static DamageAPI.ModdedDamageType AltSpecialSiphon;
        public static DamageAPI.ModdedDamageType AltUtilityFreeze;

        internal static Material frozenMat;

        internal static GameObject NeuroChargeBar;

        //ALL TEMP
        internal static GameObject captureTracerEffect;
        internal static GameObject timeSiphonOrbEffect;
        internal static GameObject timeBeamImpact;
        internal static GameObject neuroMuzzleFlash;

        internal static GameObject timePunchHitEffect;
        internal static GameObject timePunchSwingEffect;
        internal static GameObject timePunchChargeEffect;

        internal static GameObject timeBeamChargeEffect;
        internal static GameObject timeBeamEffect;
        internal static GameObject punchImpactEffect;

        internal static GameObject nearbySiphonIndicator;
        internal static GameObject kaboomEffect;

        internal static GameObject punchTracer;
        //Projectile
        internal static GameObject timeFreezeZone;
        internal static GameObject overheatBallBlast;
        internal static GameObject timeZapCone;
        internal static GameObject punchBarrage;
        internal static GameObject timeFreezeZoneStatic;
        //Sounds
        internal static NetworkSoundEventDef timePunchHitSoundDef;

        //Color
        internal static Color lightCyan = new Color(151f / 255f, 229f / 255f, 240f / 255f);
        internal static Color darkCyan = new Color(10f / 255f, 130f / 255f, 145f / 255f);

        //UI
        internal static GameObject chargeCrosshair;
        public override void Initialize()
        {
            DelayedPrimary = DamageAPI.ReserveDamageType();
            DelayedSecondary = DamageAPI.ReserveDamageType();
            DelayedUtility = DamageAPI.ReserveDamageType();
            DelayedPunch = DamageAPI.ReserveDamageType();
            AltSpecialSiphon = DamageAPI.ReserveDamageType();
            AltUtilityFreeze = DamageAPI.ReserveDamageType();

            Hooks();

            CreateEffects();

            CreateSounds();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            CreateUI();

            frozenMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/EliteHaunted/matEliteHauntedOverlay.mat").WaitForCompletion();

            NeuroChargeBar = AssetCollection.FindAsset<GameObject>("NeuroChargeBar");

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acNeuromancer", ScrapyardBundle.Indev);
        }
        private void CreateEffects()
        {
            nearbySiphonIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion().InstantiateClone("NeuroNearbySiphonEffect");
            if (!nearbySiphonIndicator.GetComponent<NetworkIdentity>()) nearbySiphonIndicator.AddComponent<NetworkIdentity>();

            nearbySiphonIndicator.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", lightCyan);
            nearbySiphonIndicator.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", lightCyan);

            kaboomEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/IceCullExplosion.prefab").WaitForCompletion().InstantiateClone("NeuroExplosionEffect");
            if (!kaboomEffect.GetComponent<NetworkIdentity>()) kaboomEffect.AddComponent<NetworkIdentity>();

            CreateAndAddEffectDef(kaboomEffect);

            captureTracerEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerCaptureTracer.prefab").WaitForCompletion().InstantiateClone("NeuromancerCaptureTracer", false);

            CreateAndAddEffectDef(captureTracerEffect);

            neuroMuzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab").WaitForCompletion().InstantiateClone("NeuromancerMuzzleFlash", false);

            CreateAndAddEffectDef(neuroMuzzleFlash);

            timeBeamImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MissileVoid/VoidImpactEffect.prefab").WaitForCompletion().InstantiateClone("TimeBeamImpactEffect", false);
            timeBeamImpact.GetComponent<EffectComponent>().soundName = "";
            timeBeamImpact.transform.Find("Scaled Hitspark 1 (Random Color)").gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark2Generic.mat").WaitForCompletion();
            timeBeamImpact.transform.Find("Scaled Hitspark 1 (Random Color)").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", darkCyan);

            timeBeamImpact.transform.Find("Scaled Hitspark 1 (Random Color) (1)").gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark1Generic.mat").WaitForCompletion();
            timeBeamImpact.transform.Find("Scaled Hitspark 1 (Random Color) (1)").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", lightCyan);

            var hi = timeBeamImpact.transform.Find("Scaled Hitspark 1 (Random Color) (1)").gameObject.GetComponent<ParticleSystem>().main;

            hi = timeBeamImpact.transform.Find("Flash, Hard").gameObject.GetComponent<ParticleSystem>().main;
            hi.startColor = lightCyan;

            timeBeamImpact.transform.Find("Impact Shockwave").gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark1Generic.mat").WaitForCompletion();
            timeBeamImpact.transform.Find("Impact Shockwave").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", lightCyan);

            CreateAndAddEffectDef(timeBeamImpact);


            timeSiphonOrbEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/HealthOrbEffect.prefab").WaitForCompletion().InstantiateClone("TimeSiphonOrbEffect", false);

            Material trail = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainTracerTrail.mat").WaitForCompletion();
            trail.SetColor("_TintColor", darkCyan);
            Material[] mat = new Material[1];
            mat[0] = trail;
            timeSiphonOrbEffect.transform.Find("TrailParent").Find("Trail").gameObject.GetComponent<TrailRenderer>().materials = mat;
            timeSiphonOrbEffect.transform.Find("TrailParent").Find("Trail").gameObject.GetComponent<TrailRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            timeSiphonOrbEffect.transform.Find("TrailParent").Find("Trail").gameObject.GetComponent<TrailRenderer>().receiveShadows = true;
            timeSiphonOrbEffect.transform.Find("TrailParent").Find("Trail").gameObject.GetComponent<TrailRenderer>().numCornerVertices = 2;

            var col = timeSiphonOrbEffect.transform.Find("VFX").Find("Core").GetComponent<ParticleSystem>().colorOverLifetime;
            col.color = new ParticleSystem.MinMaxGradient(darkCyan, lightCyan);

            CreateAndAddEffectDef(timeSiphonOrbEffect);

            timePunchHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab").WaitForCompletion().InstantiateClone("TimePunchHitEffect", false);

            CreateAndAddEffectDef(timePunchHitEffect);

            timePunchSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderFistLoop.prefab").WaitForCompletion().InstantiateClone("TimePunchLoopEffect", false);

            timePunchChargeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/ChargeLoaderFist.prefab").WaitForCompletion().InstantiateClone("TimePunchChargeEffect", false);

            timeBeamChargeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamChargeCorrupt.prefab").WaitForCompletion().InstantiateClone("TimeBeamChargeEffect", false);

            CreateAndAddEffectDef(timeBeamChargeEffect);

            timeBeamEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamCorrupt.prefab").WaitForCompletion().InstantiateClone("TimeBeamEffect", false);
            timeBeamEffect.transform.localScale *= 0.25f;

            punchImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab").WaitForCompletion().InstantiateClone("NeuroPunchImpact", true);
            if (!punchImpactEffect.GetComponent<NetworkIdentity>()) punchImpactEffect.AddComponent<NetworkIdentity>();
            punchImpactEffect.GetComponent<EffectComponent>().applyScale = true;

            punchImpactEffect.transform.Find("Flash, Hard").gameObject.SetActive(false);
            punchImpactEffect.transform.Find("Scaled Hitspark 1 (Random Color)").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat").WaitForCompletion();
            punchImpactEffect.transform.Find("Scaled Hitspark 3 (Random Color)").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/KillEliteFrenzy/matOmniHitspark3Frenzy.mat").WaitForCompletion();
            punchImpactEffect.transform.Find("ScaledSmokeRing, Mesh").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/FireBallDash/matDustOpaque.mat").WaitForCompletion();

            punchImpactEffect.transform.Find("Impact Shockwave").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniRing1Void.mat").WaitForCompletion();

            GameObject fistEffect = GameObject.Instantiate(AssetCollection.FindAsset<GameObject>("PunchEffect"));
            fistEffect.transform.parent = punchImpactEffect.transform;
            fistEffect.transform.localPosition = Vector3.zero;
            fistEffect.transform.localRotation = Quaternion.identity;
            fistEffect.transform.localScale *= 0.15f;

            CreateAndAddEffectDef(punchImpactEffect);

            punchTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion().InstantiateClone("NeuromancerPunchTracer", false);
            punchTracer.transform.localScale *= 3f;

            CreateAndAddEffectDef(punchTracer);
        }
        #region projectiles
        private void CreateProjectiles()
        {
            timeFreezeZone = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("TimeFreezeZone", false);

            BuffWard buffWard = timeFreezeZone.GetComponent<BuffWard>();
            buffWard.radius = 45f;
            buffWard.interval = 0.01f;
            buffWard.buffDef = AssetCollection.FindAsset<BuffDef>("bdTimeStopped");
            buffWard.expires = false;
            buffWard.expireDuration = 0f;
            buffWard.invertTeamFilter = true;

            timeFreezeZone.GetComponent<SphereCollider>().radius = 45f;

            timeFreezeZone.GetComponent<SlowDownProjectiles>().slowDownCoefficient = 0f;

            timeFreezeZone.AddComponent<NeuromancerTimeFieldComponent>();

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(timeFreezeZone);

            timeFreezeZoneStatic = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("TimeFreezeZoneStatic", false);

            BuffWard buffWardStatic = timeFreezeZoneStatic.GetComponent<BuffWard>();
            buffWardStatic.radius = 45f;
            buffWardStatic.interval = 0.01f;
            buffWardStatic.buffDef = AssetCollection.FindAsset<BuffDef>("bdTimeStopped");
            buffWardStatic.expires = false;
            buffWardStatic.expireDuration = 0f;
            buffWardStatic.invertTeamFilter = true;

            timeFreezeZoneStatic.GetComponent<SphereCollider>().radius = 45f;

            timeFreezeZoneStatic.GetComponent<SlowDownProjectiles>().slowDownCoefficient = 0f;

            timeFreezeZoneStatic.AddComponent<NeuromancerStaticTimeFieldComponent>();

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(timeFreezeZoneStatic);

            overheatBallBlast = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab").WaitForCompletion().InstantiateClone("NeuromancerOverheatBall");
            if (!overheatBallBlast.GetComponent<NetworkIdentity>()) overheatBallBlast.AddComponent<NetworkIdentity>();

            DamageAPI.ModdedDamageTypeHolderComponent mdthc = overheatBallBlast.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            mdthc.Add(DelayedPrimary);

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(overheatBallBlast);

            timeZapCone = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone"), "NeuromancerZapCone");
            if (!timeZapCone.GetComponent<NetworkIdentity>()) timeZapCone.AddComponent<NetworkIdentity>();

            DamageAPI.ModdedDamageTypeHolderComponent mdthc2 = timeZapCone.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            mdthc2.Add(DelayedUtility);

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(timeZapCone);

            punchBarrage = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderZapCone.prefab").WaitForCompletion().InstantiateClone("NeuromancerPunchBarrage", true);

            Component.Destroy(punchBarrage.GetComponent<ProjectileProximityBeamController>());
            Component.Destroy(punchBarrage.GetComponent<DelayedEvent>());
            Component.Destroy(punchBarrage.GetComponent<StartEvent>());
            punchBarrage.AddComponent<PunchEffectComponent>();

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(punchBarrage);
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            timePunchHitSoundDef = CreateAndAddNetworkSoundEventDef("Play_loader_m2_impact");
        }
        #endregion

        #region UI
        private static void CreateUI()
        {
            chargeCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StraightBracketCrosshair.prefab").WaitForCompletion();
        }
        #endregion
        private static void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            HUD.onHudTargetChangedGlobal += HUDSetup;
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
            if (NetworkServer.active)
            {
                if (victimBody && attackerBody)
                {
                    if (damageInfo.HasModdedDamageType(AltUtilityFreeze))
                    {
                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdTimeStopped, 1.5f);
                    }
                }
            }
        }
        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimBody = self.body;

                    if (attackerBody && victimBody)
                    {
                        if (victimBody.HasBuff(ScrapyardContent.Buffs.bdTimeStopped))
                        {
                            DelayedDamageController delayedDamageController;
                            if (!victimBody.gameObject.GetComponent<DelayedDamageController>()) delayedDamageController = victimBody.gameObject.AddComponent<DelayedDamageController>();
                            else delayedDamageController = victimBody.gameObject.GetComponent<DelayedDamageController>();

                            delayedDamageController.damageInfos.Add(damageInfo);
                            delayedDamageController.attackedVector.Add(victimBody.corePosition - attackerBody.corePosition);

                            damageInfo.rejected = true;
                        }
                        else if (damageInfo.HasModdedDamageType(DelayedSecondary))
                        {
                            NeuromancerController nController = damageInfo.attacker.GetComponent<NeuromancerController>();
                            if (nController)
                            {
                                TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                                timeOrb.origin = victimBody.corePosition;
                                timeOrb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                                timeOrb.siphonValue = nController.maxTimeEssence / 4f;
                                timeOrb.overrideDuration = 0.3f;
                                RoR2.Orbs.OrbManager.instance.AddOrb(timeOrb);
                            }
                        }
                        else if (damageInfo.HasModdedDamageType(DelayedPunch))
                        {

                            NeuromancerController nController = damageInfo.attacker.GetComponent<NeuromancerController>();
                            if (nController)
                            {
                                TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                                timeOrb.origin = victimBody.corePosition;
                                timeOrb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                                timeOrb.siphonValue = nController.maxTimeEssence / 64f * damageInfo.procCoefficient;
                                timeOrb.overrideDuration = 0.3f;
                                RoR2.Orbs.OrbManager.instance.AddOrb(timeOrb);
                            }

                            EffectManager.SpawnEffect(punchImpactEffect, new EffectData
                            {
                                origin = self.gameObject.transform.position,
                                scale = 1.5f
                            }, false);

                            Util.PlaySound("sfx_neuromancer_punch", self.gameObject);
                        }
                        else if (damageInfo.HasModdedDamageType(DelayedUtility))
                        {
                            NeuromancerController nController = damageInfo.attacker.GetComponent<NeuromancerController>();
                            if (nController)
                            {
                                TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                                timeOrb.origin = victimBody.corePosition;
                                timeOrb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                                timeOrb.siphonValue = nController.maxTimeEssence / 8f;
                                timeOrb.overrideDuration = 0.3f;
                                RoR2.Orbs.OrbManager.instance.AddOrb(timeOrb);
                            }
                        }
                        else if (damageInfo.HasModdedDamageType(DelayedPrimary))
                        {
                            damageInfo.RemoveModdedDamageType(DelayedPrimary);

                            NeuromancerController nController = damageInfo.attacker.GetComponent<NeuromancerController>();
                            if (nController)
                            {
                                TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                                timeOrb.origin = victimBody.corePosition;
                                timeOrb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                                timeOrb.siphonValue = nController.maxTimeEssence / 32f * damageInfo.procCoefficient;
                                timeOrb.overrideDuration = 0.3f;
                                RoR2.Orbs.OrbManager.instance.AddOrb(timeOrb);
                            }
                        }
                        else if (damageInfo.HasModdedDamageType(AltSpecialSiphon))
                        {
                            damageInfo.RemoveModdedDamageType(AltSpecialSiphon);

                            NeuromancerController nController = damageInfo.attacker.GetComponent<NeuromancerController>();
                            if (nController)
                            {
                                TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                                timeOrb.origin = victimBody.corePosition;
                                timeOrb.target = damageInfo.attacker.GetComponent<CharacterBody>().mainHurtBox;
                                timeOrb.siphonValue = nController.maxTimeEssence / 32f * damageInfo.procCoefficient;
                                timeOrb.overrideDuration = 0.3f;
                                RoR2.Orbs.OrbManager.instance.AddOrb(timeOrb);
                            }
                        }
                    }
                }
            }
            orig.Invoke(self, damageInfo);
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(ScrapyardContent.Buffs.bdTimeStopped))
            {
                if (sender.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                {
                    TimeStoppedState timeStoppedState = new TimeStoppedState();
                    setStateOnHurt.targetStateMachine.SetInterruptState(timeStoppedState, InterruptPriority.Frozen);

                    EntityStateMachine[] array = setStateOnHurt.idleStateMachine;

                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].SetNextState(new Idle());
                    }
                }
                else if (sender.TryGetComponent(out EntityStateMachine entityMachine))
                {
                    TimeStoppedState timeStoppedState = new TimeStoppedState();
                    entityMachine.SetInterruptState(timeStoppedState, InterruptPriority.Frozen);
                }

                args.moveSpeedRootCount += 1;
            }
        }
        internal static void HUDSetup(HUD hud)
        {
            if (hud.targetBodyObject && hud.targetMaster && hud.targetMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex == BodyCatalog.FindBodyIndex("NeuromancerBody"))
            {
                if (!hud.targetMaster.hasAuthority) return;

                Transform skillsContainer = hud.equipmentIcons[0].gameObject.transform.parent;

                // ammo display for atomic
                Transform healthbarContainer = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("BarRoots").Find("LevelDisplayCluster");

                GameObject nTrack = GameObject.Instantiate(healthbarContainer.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster"));
                nTrack.name = "AmmoTracker";
                nTrack.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                GameObject.DestroyImmediate(nTrack.transform.GetChild(0).gameObject);
                MonoBehaviour.Destroy(nTrack.GetComponentInChildren<LevelText>());
                MonoBehaviour.Destroy(nTrack.GetComponentInChildren<ExpBar>());

                nTrack.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(false);
                GameObject.DestroyImmediate(nTrack.transform.Find("ExpBarRoot").gameObject);

                nTrack.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform rect = nTrack.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.8f, 0.8f, 1f);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(120f, -40f);
                rect.offsetMax = new Vector2(120f, -40f);
                rect.pivot = new Vector2(0.5f, 0f);
                //positional data doesnt get sent to clients? Manually making offsets works..
                rect.anchoredPosition = new Vector2(50f, 0f);
                rect.localPosition = new Vector3(120f, -40f, 0f);

                GameObject chargeBarAmmo = GameObject.Instantiate(NeuroChargeBar);
                chargeBarAmmo.name = "TimeEssenceGauge";
                chargeBarAmmo.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                rect = chargeBarAmmo.GetComponent<RectTransform>();

                rect.localScale = new Vector3(0.75f, 0.1f, 1f);
                rect.anchorMin = new Vector2(100f, 2f);
                rect.anchorMax = new Vector2(100f, 2f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(100f, 2f);
                rect.localPosition = new Vector3(100f, 2f, 0f);
                rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

                TimeEssenceGauge atomicTrackerComponent = nTrack.AddComponent<TimeEssenceGauge>();

                atomicTrackerComponent.targetHUD = hud;
                atomicTrackerComponent.targetText = nTrack.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                atomicTrackerComponent.durationDisplay = chargeBarAmmo;
                atomicTrackerComponent.durationBar = chargeBarAmmo.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                atomicTrackerComponent.durationBarRed = chargeBarAmmo.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
                atomicTrackerComponent.durationBarOver = chargeBarAmmo.transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
        }
        private static void CreateAndAddEffectDef(GameObject effect)
        {
            EffectDef effectDef = new EffectDef(effect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef);
        }

        internal static void AddNetworkSoundEventDef(NetworkSoundEventDef networkSoundEventDef)
        {
            ScrapyardContent.scrapyardContentPack.networkSoundEventDefs.AddSingle(networkSoundEventDef);
        }
        internal static NetworkSoundEventDef CreateAndAddNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            AddNetworkSoundEventDef(networkSoundEventDef);

            return networkSoundEventDef;
        }
    }
}

