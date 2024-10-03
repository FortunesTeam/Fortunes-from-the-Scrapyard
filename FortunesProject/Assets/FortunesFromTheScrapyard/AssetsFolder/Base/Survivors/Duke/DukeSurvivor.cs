using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Orbs;
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
using FortunesFromTheScrapyard.Ricochet;

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
        public static float baseSalvoDamageCoefficient = 3.75f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(MINETOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        internal static float damageShareCoefficient = 1.05f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(CLONETOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseCloneDamageCoefficient = 5f;

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(SALVOTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseFanDamageCoefficient = 3f;

        public static DamageAPI.ModdedDamageType DukeFourthShot;
        public static DamageAPI.ModdedDamageType DukeRicochet;

        //ALL TEMP
        internal static GameObject dukeTracer;
        internal static GameObject dukeTracerCrit;

        internal static GameObject dukeBoomEffect;

        internal static GameObject bullet;
        internal static GameObject gun;
        internal static GameObject casing;

        internal static GameObject dukePistolSpinEffect;

        internal static GameObject ricochetTracer;
        internal static GameObject ricochetImpact;
        internal static GameObject ricochetOrbEffect;

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
            DukeRicochet = DamageAPI.ReserveDamageType();

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
        private void CreateEffects()
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

            #region ricochet
            ricochetTracer = assetCollection.FindAsset<GameObject>("RicochetTracer");
            ricochetTracer.AddComponent<NetworkIdentity>();

            var effect1 = ricochetTracer.AddComponent<EffectComponent>();
            effect1.parentToReferencedTransform = false;
            effect1.positionAtReferencedTransform = false;
            effect1.applyScale = false;
            effect1.disregardZScale = false;

            ricochetTracer.AddComponent<EventFunctions>();
            var tracer = ricochetTracer.AddComponent<RicochetTracer>();
            tracer.startTransform = ricochetTracer.transform.Find("Trail").Find("TrailTail");
            tracer.beamObject = ricochetTracer.transform.Find("Trail").Find("TrailTail").gameObject;
            tracer.beamDensity = 0.2f;
            tracer.speed = 1000f;
            tracer.headTransform = ricochetTracer.transform.Find("Tail");
            tracer.tailTransform = ricochetTracer.transform.Find("Trail").Find("TrailTail");
            tracer.length = 20f;

            var destroyOnTimer = ricochetTracer.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 2;
            var trailChildObject = ricochetTracer.transform.Find("Trail").gameObject;

            var beamPoints = trailChildObject.AddComponent<BeamPointsFromTransforms>();
            beamPoints.target = trailChildObject.GetComponent<LineRenderer>();
            Transform[] bleh = new Transform[2];
            bleh[0] = ricochetTracer.transform.Find("Tail");
            bleh[1] = trailChildObject.transform.Find("Head");
            beamPoints.pointTransforms = bleh;
            trailChildObject.GetComponent<LineRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainTracerTrail.mat").WaitForCompletion();
            trailChildObject.GetComponent<LineRenderer>().material.SetColor("_TintColor", Color.yellow);
            var animateShader = trailChildObject.AddComponent<AnimateShaderAlpha>();
            var curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.675f, 0.8f), new Keyframe(1, 0.3f));
            curve.preWrapMode = WrapMode.Clamp;
            curve.postWrapMode = WrapMode.Clamp;
            animateShader.alphaCurve = curve;
            animateShader.timeMax = 0.5f;
            animateShader.pauseTime = false;
            animateShader.destroyOnEnd = true;
            animateShader.disableOnEnd = false;

            ScrapyardContent.CreateAndAddEffectDef(ricochetTracer);

            ricochetImpact = assetCollection.FindAsset<GameObject>("RicochetImpactHit");
            var attr = ricochetImpact.AddComponent<VFXAttributes>();
            attr.vfxPriority = VFXAttributes.VFXPriority.Low;
            attr.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            ricochetImpact.AddComponent<EffectComponent>();
            ricochetImpact.AddComponent<DestroyOnParticleEnd>();

            var eff = ricochetImpact.transform.Find("Streaks_Ps").GetComponent<ParticleSystemRenderer>();
            eff.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Firework/matFireworkSparkle.mat").WaitForCompletion();
            eff.material.SetColor("_TintColor", Color.yellow);
            eff = ricochetImpact.transform.Find("Flash_Ps").GetComponent<ParticleSystemRenderer>();
            eff.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarSkillReplacements/matBirdHeartRuin.mat").WaitForCompletion();
            eff.material.SetColor("_TintColor", Color.yellow);

            ScrapyardContent.CreateAndAddEffectDef(ricochetImpact);

            ricochetOrbEffect = assetCollection.FindAsset<GameObject>("RicochetOrbEffect");
            ricochetOrbEffect.AddComponent<EventFunctions>();
            var effectComp = ricochetOrbEffect.AddComponent<EffectComponent>();
            effectComp.applyScale = true;
            var orbEffect = ricochetOrbEffect.AddComponent<RicochetOrbEffect>();

            curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            curve.preWrapMode = WrapMode.Clamp;
            curve.postWrapMode = WrapMode.Clamp;

            orbEffect.movementCurve = curve;
            orbEffect.faceMovement = true;
            orbEffect.callArrivalIfTargetIsGone = true;
            orbEffect.endEffect = ricochetOrbEffect;
            orbEffect.endEffectCopiesRotation = false;

            attr = ricochetOrbEffect.AddComponent<VFXAttributes>();
            attr.vfxPriority = VFXAttributes.VFXPriority.Always;
            attr.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            ricochetOrbEffect.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Captain/matCaptainTracerTrail.mat").WaitForCompletion();
            ricochetOrbEffect.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().material.SetColor("_TintColor", Color.yellow);

            var pscfed = ricochetOrbEffect.AddComponent<ParticleSystemColorFromEffectData>();
            pscfed.particleSystems = new ParticleSystem[1];
            pscfed.particleSystems[0] = ricochetOrbEffect.transform.Find("Head").GetComponent<ParticleSystem>();
            pscfed.effectComponent = effectComp;

            var trcfed = ricochetOrbEffect.AddComponent<TrailRendererColorFromEffectData>();
            trcfed.renderers = new TrailRenderer[1];
            trcfed.renderers[0] = ricochetOrbEffect.transform.Find("Trail").GetComponent<TrailRenderer>();
            trcfed.effectComponent = effectComp;

            var shaderAlpha = ricochetOrbEffect.transform.Find("Trail").gameObject.AddComponent<AnimateShaderAlpha>();

            curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
            curve.preWrapMode = WrapMode.Clamp;
            curve.postWrapMode = WrapMode.Clamp;

            shaderAlpha.alphaCurve = curve;
            shaderAlpha.timeMax = 0.75f;
            shaderAlpha.pauseTime = false;
            shaderAlpha.destroyOnEnd = true;
            shaderAlpha.disableOnEnd = false;

            var effect = ricochetOrbEffect.transform.Find("Head").GetComponent<ParticleSystemRenderer>();
            effect.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Firework/matFireworkSparkle.mat").WaitForCompletion();
            effect.material.SetColor("_TintColor", Color.yellow);

            ScrapyardContent.CreateAndAddEffectDef(ricochetOrbEffect);
            #endregion
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
            ps.desiredForwardSpeed = 150f;

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

            GameObject dukeColliderObject = new GameObject();
            dukeColliderObject.layer = LayerIndex.transparentFX.intVal;
            SphereCollider dukeFieldCollider = dukeColliderObject.AddComponent<SphereCollider>();
            dukeFieldCollider.radius = 15f;
            dukeFieldCollider.isTrigger = false;

            GameObject dukeTriggerObject = new GameObject();
            dukeTriggerObject.layer = LayerIndex.entityPrecise.intVal;
            SphereCollider dukeTriggerCollider = dukeColliderObject.AddComponent<SphereCollider>();
            dukeTriggerCollider.radius = 15f;
            dukeTriggerCollider.isTrigger = true;

            dukeTriggerObject.transform.SetParent(dukeColliderObject.transform, false);

            DisableCollisionsIfInTrigger dCIIN = dukeTriggerObject.AddComponent<DisableCollisionsIfInTrigger>();
            dCIIN.colliderToIgnore = dukeFieldCollider;

            dukeColliderObject.transform.SetParent(dukeField.transform, false);

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
            On.RoR2.GlobalEventManager.OnHitAllProcess += GlobalEventManager_OnHitAllProcess;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
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
                    #region FourthShot
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

                        if (attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("DukeBody") && attackerBody.skillLocator.utility)
                        {
                            if (attackerBody.skillLocator.utility.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Flourish")))
                            {
                                NetworkIdentity networkIdentity = attackerBody.networkIdentity;

                                new SyncDukeRecharge(networkIdentity.netId).Send(R2API.Networking.NetworkDestination.Clients);
                            }
                        }
                    }
                    #endregion

                    #region Ricochet
                    if(victimBody.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare))
                    {
                        var victimTrc = victimBody.gameObject.EnsureComponent<TemporaryRicochetControllerVictim>();

                        if (!damageInfo.HasModdedDamageType(DukeRicochet))
                        {
                            RicochetOrb orb = new RicochetOrb
                            {
                                originalPosition = damageInfo.position,
                                origin = damageInfo.position,
                                speed = 250f,
                                attacker = damageInfo.attacker,
                                damageValue = damageInfo.damage * victimTrc.bounceCountStored,
                                teamIndex = attackerBody.teamComponent.teamIndex,
                                procCoefficient = damageInfo.procCoefficient,
                                isCrit = damageInfo.crit,
                                bounceCount = victimTrc.bounceCountStored,
                                hitObjects = new System.Collections.Generic.List<GameObject>(),
                                damageColorIndex = DamageColorIndex.WeakPoint
                            };
                            orb.hitObjects.Add(victim.gameObject);

                            if (victimTrc) Component.Destroy(victimTrc);

                            OrbManager.instance.AddOrb(orb);
                        }
                        else
                        {
                            victimTrc.RicochetBullet(damageInfo);
                        }
                        
                    }
                    #endregion
                }
            }
        }

        private void GlobalEventManager_OnHitAllProcess(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            damageInfo.attacker.TryGetComponent<CharacterBody>(out var attackerBody);

            if(attackerBody && !damageInfo.HasModdedDamageType(DukeRicochet) && 
                hitObject.transform.root.gameObject.TryGetComponent<BuffWard>(out var buffWard) && 
                buffWard && buffWard.buffDef == ScrapyardContent.Buffs.bdDukeDamageShare)
            {
                RicochetOrb orb = new RicochetOrb
                {
                    originalPosition = hitObject.transform.position,
                    origin = hitObject.transform.position,
                    speed = 500f,
                    attacker = damageInfo.attacker,
                    damageValue = damageInfo.damage *= damageShareCoefficient,
                    teamIndex = attackerBody.teamComponent.teamIndex,
                    procCoefficient = damageInfo.procCoefficient,
                    isCrit = damageInfo.crit,
                    bounceCount = 0,
                    hitObjects = new System.Collections.Generic.List<GameObject>(),
                    damageColorIndex = DamageColorIndex.WeakPoint
                };
                orb.hitObjects.Add(hitObject);
                
                OrbManager.instance.AddOrb(orb);
            }
            orig.Invoke(self, damageInfo, hitObject);
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
    }
}

