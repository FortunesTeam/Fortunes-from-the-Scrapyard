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

namespace FortunesFromTheScrapyard.Survivors.Duke
{
    public class DukeSurvivor : ScrapyardSurvivor
    {
        public static DamageAPI.ModdedDamageType DukeFourthShot;
        public static DamageAPI.ModdedDamageType DukeSharedDamageType;

        //ALL TEMP
        internal static GameObject dukeTracer;
        internal static GameObject dukeTracerCrit;

        internal static GameObject dukeBoomEffect;

        internal static GameObject bullet;
        internal static GameObject gun;
        internal static GameObject casing;

        //Projectile
        internal static GameObject damageShareMine;
        internal static GameObject dukeField;
        //Sounds
        //Color
        internal static Color orange = new Color(255f / 255f, 127f / 255f, 80f / 255f);

        //UI
        internal static GameObject chargeCrosshair;
        public override void Initialize()
        {
            DukeFourthShot = DamageAPI.ReserveDamageType();
            DukeSharedDamageType = DamageAPI.ReserveDamageType();

            Hooks();

            CreateEffects();

            BodyCatalog.availability.CallWhenAvailable(CreateProjectiles);

            CreateUI();

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/Bandit2/Crosshair");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acDuke", ScrapyardBundle.Indev);
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
        }

        #endregion
        #region projectiles
        private static void CreateProjectiles()
        {

            damageShareMine = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAlt.prefab").WaitForCompletion().InstantiateClone("DukeDamageShareMine");

            ProjectileStickOnImpact sticky = damageShareMine.GetComponent<ProjectileStickOnImpact>();
            sticky.ignoreCharacters = false;

            ProjectileExplosion pe = damageShareMine.GetComponent<ProjectileExplosion>();

            dukeField = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab").WaitForCompletion().InstantiateClone("DukeDamageField");

            BuffWard buffWard = dukeField.GetComponent<BuffWard>();
            buffWard.invertTeamFilter = true;
            buffWard.buffDef = ScrapyardContent.Buffs.bdDukeDamageShare;
            buffWard.interval = 0.01f;
            buffWard.expireDuration = 5f;
            buffWard.radius = 25f;

            dukeField.GetComponent<SphereCollider>().radius = 25f;

            UnityEngine.Object.Destroy(dukeField.GetComponent<SlowDownProjectiles>());

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(dukeField);

            pe.childrenProjectilePrefab = dukeField;

            ScrapyardContent.scrapyardContentPack.projectilePrefabs.AddSingle(damageShareMine);
        }
        #endregion

        #region sounds
        #endregion

        #region UI
        private static void CreateUI()
        {
            chargeCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StraightBracketCrosshair.prefab").WaitForCompletion();
        }
        #endregion
        private void Hooks()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            if (ScrapyardMain.emotesInstalled)
            {
                Emotes();
            }
        }
        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig.Invoke(self);

            if (self)
            {
                if (self.HasBuff(ScrapyardContent.Buffs.bdDukeSpeedBuff))
                {
                    self.moveSpeed += 0.25f * self.GetBuffCount(ScrapyardContent.Buffs.bdDukeSpeedBuff);
                }

                if (self.bodyIndex == BodyCatalog.FindBodyIndex("DukeBody") || self.bodyIndex == BodyCatalog.FindBodyIndex("DukeDecoyBody"))
                {
                    DukeController dukeController = self.GetComponent<DukeController>();
                    if (dukeController != null)
                    {
                        float baseAttackSpeed = self.baseAttackSpeed + (self.levelAttackSpeed * self.level);
                        float baseDamage = self.baseDamage + (self.baseDamage * self.level);
                        float newDamage = self.damage * ((self.attackSpeed - baseAttackSpeed) * 0.7f);
                        self.damage += newDamage;
                        self.attackSpeed = (self.attackSpeed - baseAttackSpeed) * 0.3f + baseAttackSpeed;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Indev).LoadAsset<GameObject>("duke_emoteskeleton");
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
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: false);
                }
            }
            else
            {
                if (mapper.transform.name == "duke_emoteskeleton")
                {
                    mapper.transform.parent.Find("meshGun").gameObject.SetActive(value: true);
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
                        int num6 = 5;
                        float harpoon = 2.5f;
                        attackerBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeSpeedBuff);
                        for (int l = 0; l < num6; l++)
                        {
                            attackerBody.AddTimedBuff(ScrapyardContent.Buffs.bdDukeSpeedBuff, harpoon * (float)(l + 1) / (float)num6);
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
                    }

                    if (!damageInfo.HasModdedDamageType(DukeSharedDamageType) && victimBody.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare))
                    {
                        foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
                        {
                            if (body.teamComponent.teamIndex != attackerBody.teamComponent.teamIndex && body != victimBody && body.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare))
                            {
                                DamageInfo dukeSharedDamage = new DamageInfo();
                                dukeSharedDamage.attacker = damageInfo.attacker;
                                dukeSharedDamage.inflictor = damageInfo.inflictor;
                                dukeSharedDamage.damage = damageInfo.damage * 0.35f;
                                dukeSharedDamage.procCoefficient = 0.7f;
                                dukeSharedDamage.crit = false;
                                dukeSharedDamage.damageType = damageInfo.damageType;
                                dukeSharedDamage.damageColorIndex = DamageColorIndex.WeakPoint;
                                dukeSharedDamage.force = Vector3.zero;
                                dukeSharedDamage.position = body.corePosition;
                                dukeSharedDamage.AddModdedDamageType(DukeSharedDamageType);

                                body.healthComponent.TakeDamage(dukeSharedDamage);
                            }
                        }
                    }
                    
                    if(attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("DukeBody") && damageInfo.crit && (damageInfo.damageType & DamageType.BonusToLowHealth) != 0 && 
                        attackerBody.skillLocator.utility.skillDef.skillIndex == SkillCatalog.FindSkillIndexByName("Flourish"))
                    {
                        attackerBody.skillLocator.utility.RunRecharge(1.5f);
                    }
                }
            }
        }
    }
}

