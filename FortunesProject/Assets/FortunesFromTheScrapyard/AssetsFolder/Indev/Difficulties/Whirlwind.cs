using MSU.Config;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using RoR2.CharacterAI;
using RoR2.Projectile;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using BepInEx;
using System.Collections.Generic;

namespace FortunesFromTheScrapyard
{
    public class Whirlwind : ScrapyardDifficulty
    {
        public override ScrapyardAssetRequest<SerializableDifficultyDef> AssetRequest => ScrapyardAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", ScrapyardBundle.Indev);

        public static SerializableDifficultyDef whirlwindDifficulty;

        internal static bool prediction = true;
        internal static float attackSpeed = 1.5f;
        internal static float moveSpeed = 1.3f;
        internal static float cdr = 0.5f;
        internal static float teleporterRadius = -30f;
        public override void Initialize()
        {
            whirlwindDifficulty = DifficultyDef;
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnRunEnd(Run run)
        {
            if (DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty) == whirlwindDifficulty.DifficultyDef)
            {
                On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;

                On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;

                On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;

                Lemurian.UnInit();
                Vulture.UnInit();
                Bronzong.UnInit();
                GreaterWisp.UnInit();
                BeetleGuard.UnInit();
                ClayGrenadier.UnInit();
                LemurianBruiser.UnInit();
                Scavenger.UnInit();
                Vagrant.UnInit();
                VoidJailer.UnInit();
                ClayBoss.UnInit();
                Grovetender.UnInit();
                RoboBallBoss.UnInit();
                FlyingVermin.UnInit();
                MinorConstruct.UnInit();
                LunarExploder.UnInit();

                AllowPostLoopElites(false);
            }
        }

        public override void OnRunStart(Run run)
        {
            if(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty) == whirlwindDifficulty.DifficultyDef)
            {
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;

                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

                On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;

                foreach (CharacterMaster cm in run.userMasters.Values)
                    if (NetworkServer.active)
                        cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);

                Lemurian.Init();
                Vulture.Init();
                Bronzong.Init();
                GreaterWisp.Init();
                BeetleGuard.Init();
                ClayGrenadier.Init();
                LemurianBruiser.Init();
                Scavenger.Init();
                Vagrant.Init();
                VoidJailer.Init();
                ClayBoss.Init();
                Grovetender.Init();
                RoboBallBoss.Init();
                FlyingVermin.Init();
                MinorConstruct.Init();
                LunarExploder.Init();

                AllowPostLoopElites(true);
            }

        }
        private static void AllowPostLoopElites(bool enable)
        {
            HashSet<EliteDef> hashSet = new HashSet<EliteDef>();
            CombatDirector.EliteTierDef[] eliteTiers = CombatDirector.eliteTiers;
            foreach (CombatDirector.EliteTierDef eliteTierDef in eliteTiers)
            {
                if (!eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Poison) && !eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Haunted) && !eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Lunar))
                {
                    continue;
                }

                if(enable)
                {
                    eliteTierDef.isAvailable = delegate (SpawnCard.EliteRules rule)
                    {
                        if (rule != 0)
                        {
                            return false;
                        }
                        Run instance = Run.instance;
                        return instance != null || TeleporterInteraction.instance?.currentState is TeleporterInteraction.ChargingState;
                    };
                }
                else
                {
                    if(eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Lunar))
                    {
                        eliteTierDef.isAvailable = delegate (SpawnCard.EliteRules rules)
                        {
                            if (rules != 0)
                            {
                                return false;
                            }
                            Run instance = Run.instance;
                            return rules == SpawnCard.EliteRules.Lunar;
                        };
                    }
                    else if(eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Poison) && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Haunted))
                    {
                        eliteTierDef.isAvailable = delegate (SpawnCard.EliteRules rules)
                        {
                            if (rules != 0)
                            {
                                return false;
                            }
                            Run instance = Run.instance;
                            return instance.loopClearCount > 0 && rules == SpawnCard.EliteRules.Default;
                        };
                    }
                }

                if(!eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Lunar))
                {
                    if(enable)
                    {
                        EliteDef[] eliteTypes = eliteTierDef.eliteTypes;
                        foreach (EliteDef eliteDef in eliteTypes)
                        {
                            if (eliteDef && hashSet.Add(eliteDef))
                            {
                                eliteDef.damageBoostCoefficient /= 2f;
                                eliteDef.healthBoostCoefficient /= 8f;
                            }
                        }
                    }
                    else
                    {
                        EliteDef[] eliteTypes = eliteTierDef.eliteTypes;
                        foreach (EliteDef eliteDef in eliteTypes)
                        {
                            if (eliteDef && hashSet.Add(eliteDef))
                            {
                                eliteDef.damageBoostCoefficient = 6f;
                                eliteDef.healthBoostCoefficient = 18f;
                            }
                        }
                    }
                }
            }
        }

        private void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig.Invoke(self);
            self.calcRadius += Self_calcRadius;
        }
        public static void Self_calcRadius(ref float radius)
        {
            radius *= Mathf.Max(1f + teleporterRadius / 100f, 0f);
        }
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.teamComponent.teamIndex == TeamIndex.Monster)
            {
                if (self.baseNameToken != "BROTHER_BODY_NAME")
                {
                    self.moveSpeed *= moveSpeed;
                }
                self.attackSpeed *= attackSpeed;

                if (self.skillLocator)
                {
                    if (self.skillLocator.primary) self.skillLocator.primary.cooldownScale *= cdr;
                    if (self.skillLocator.secondary) self.skillLocator.secondary.cooldownScale *= cdr;
                    if (self.skillLocator.utility) self.skillLocator.utility.cooldownScale *= cdr;
                    if (self.skillLocator.special) self.skillLocator.special.cooldownScale *= cdr;
                }
            }
        }
        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            self.creditMultiplier *= 1.5f;
            self.expRewardCoefficient *= 0.75f;
            self.goldRewardCoefficient *= 0.75f;
            orig(self);
        }

        #region Prediction
        public class BeetleGuard
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.BeetleGuardMonster.FireSunder, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                aimRay.origin = self.handRTransform.position;//Called in Vanilla method, but  call here beforehand before calculating the new aimray.
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPCC(aimRay, self.GetTeam(), 45f, EntityStates.BeetleGuardMonster.FireSunder.projectilePrefab, targetHurtbox);
                                //Feed it the projectile prefab in case a mod is changing the speed.
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.BeetleGuardMonster.FireSunder, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                aimRay.origin = self.handRTransform.position;//Called in Vanilla method, but  call here beforehand before calculating the new aimray.
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPCC(aimRay, self.GetTeam(), 45f, EntityStates.BeetleGuardMonster.FireSunder.projectilePrefab, targetHurtbox);
                                //Feed it the projectile prefab in case a mod is changing the speed.
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate IL Hook failed");
                    }
                };
            }
        }
        public class Bronzong
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.Bell.BellWeapon.ChargeTrioBomb, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                //Uncomment this to improve accuracy further.
                                /*Transform t = self.FindTargetChildTransformFromBombIndex();
                                if (t)
                                {
                                    aimRay.origin = t.position;
                                }*/
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.Bell.BellWeapon.ChargeTrioBomb.bombProjectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.Bell.BellWeapon.ChargeTrioBomb, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                //Uncomment this to improve accuracy further.
                                /*Transform t = self.FindTargetChildTransformFromBombIndex();
                                if (t)
                                {
                                    aimRay.origin = t.position;
                                }*/
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.Bell.BellWeapon.ChargeTrioBomb.bombProjectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
                    }
                };
            }
        }
        public class ClayBoss
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianMonster.FireFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianMonster.FireFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade IL Hook failed");
                    }
                };
            }
        }

        public class ClayGrenadier
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                On.EntityStates.ClayGrenadier.ThrowBarrel.ModifyProjectileAimRay += (orig, self, aimRay) =>
                {
                    if (AllowPrediction(self.characterBody, loopOnly))
                    {
                        HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                        Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                        return orig(self, newAimRay);
                    }
                    return orig(self, aimRay);
                };
            }
            public static void UnInit()
            {
                On.EntityStates.ClayGrenadier.ThrowBarrel.ModifyProjectileAimRay -= (orig, self, aimRay) =>
                {
                    if (AllowPrediction(self.characterBody, loopOnly))
                    {
                        HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                        Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                        return orig(self, newAimRay);
                    }
                    return orig(self, aimRay);
                };
            }
        }

        public class FlyingVermin
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (self.GetType() == typeof(EntityStates.FlyingVermin.Weapon.Spit))
                            {
                                if (AllowPrediction(self.characterBody, loopOnly))
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: FlyingVermin EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.GenericProjectileBaseState.FireProjectile -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (self.GetType() == typeof(EntityStates.FlyingVermin.Weapon.Spit))
                            {
                                if (AllowPrediction(self.characterBody, loopOnly))
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: FlyingVermin EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
        }
        public class GreaterWisp
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GreaterWispMonster.FireCannons, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.GreaterWispMonster.FireCannons.OnEnter IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GreaterWispMonster.FireCannons, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.GreaterWispMonster.FireCannons.OnEnter IL Hook failed");
                    }
                };
            }
        }
        public class Grovetender
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.GravekeeperBoss.FireHook.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GravekeeperBoss.FireHook, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.GravekeeperBoss.FireHook.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.GravekeeperBoss.FireHook.OnEnter IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.GravekeeperBoss.FireHook.OnEnter -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GravekeeperBoss.FireHook, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.GravekeeperBoss.FireHook.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.GravekeeperBoss.FireHook.OnEnter IL Hook failed");
                    }
                };
            }
        }
        public class Lemurian
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.LemurianMonster.FireFireball.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianMonster.FireFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.LemurianMonster.FireFireball.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.LemurianMonster.FireFireball.OnEnter IL Hook failed");
                    }
                };
            }

            public static void UnInit()
            {
                IL.EntityStates.LemurianMonster.FireFireball.OnEnter -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianMonster.FireFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.LemurianMonster.FireFireball.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.LemurianMonster.FireFireball.OnEnter IL Hook failed");
                    }
                };
            }
        }
        public class LemurianBruiser
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianBruiserMonster.FireMegaFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay;

                                float projectileSpeed = EntityStates.LemurianBruiserMonster.FireMegaFireball.projectileSpeed;
                                if (projectileSpeed > 0f)
                                {
                                    newAimRay = PredictAimray(aimRay, self.GetTeam(), 45f, projectileSpeed, targetHurtbox);
                                }
                                else
                                {
                                    newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectilePrefab, targetHurtbox);
                                }

                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.LemurianBruiserMonster.FireMegaFireball, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay;

                                float projectileSpeed = EntityStates.LemurianBruiserMonster.FireMegaFireball.projectileSpeed;
                                if (projectileSpeed > 0f)
                                {
                                    newAimRay = PredictAimray(aimRay, self.GetTeam(), 45f, projectileSpeed, targetHurtbox);
                                }
                                else
                                {
                                    newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectilePrefab, targetHurtbox);
                                }

                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate IL Hook failed");
                    }
                };
            }
        }
        public class LunarExploder
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                if (self.characterBody && !self.characterBody.isPlayerControlled)
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: LunarExploder EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.GenericProjectileBaseState.FireProjectile -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                if (self.characterBody && !self.characterBody.isPlayerControlled)
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: LunarExploder EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
        }
        public class MinorConstruct
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                if (self.characterBody && !self.characterBody.isPlayerControlled)
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: MinorConstruct EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.GenericProjectileBaseState.FireProjectile -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.GenericProjectileBaseState, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                if (self.characterBody && !self.characterBody.isPlayerControlled)
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: MinorConstruct EntityStates.GenericProjectileBaseState.FireProjectile IL Hook failed");
                    }
                };
            }
        }
        public class RoboBallBoss
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.RoboBallBoss.Weapon.FireEyeBlast, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                Ray newAimRay;
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                float projectileSpeed = self.projectileSpeed;
                                if (projectileSpeed > 0f)
                                {
                                    newAimRay = PredictAimray(aimRay, self.GetTeam(), 45f, projectileSpeed, targetHurtbox);
                                }
                                else
                                {
                                    newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                }
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.RoboBallBoss.Weapon.FireEyeBlast, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                Ray newAimRay;
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                float projectileSpeed = self.projectileSpeed;
                                if (projectileSpeed > 0f)
                                {
                                    newAimRay = PredictAimray(aimRay, self.GetTeam(), 45f, projectileSpeed, targetHurtbox);
                                }
                                else
                                {
                                    newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                                }
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate IL Hook failed");
                    }
                };
            }
        }
        public class Scavenger
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.ScavMonster.FireEnergyCannon, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.ScavMonster.FireEnergyCannon.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.ScavMonster.FireEnergyCannon.OnEnter IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.ScavMonster.FireEnergyCannon, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.ScavMonster.FireEnergyCannon.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.ScavMonster.FireEnergyCannon.OnEnter IL Hook failed");
                    }
                };
            }
        }
        public class Vagrant
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate += (il) =>
                {
                    bool error = true;
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.VagrantMonster.Weapon.JellyBarrage, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.VagrantMonster.Weapon.JellyBarrage.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                        if (c.TryGotoNext(MoveType.After,
                             x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                            ))
                        {
                            c.Emit(OpCodes.Ldarg_0);
                            c.EmitDelegate<Func<Ray, EntityStates.VagrantMonster.Weapon.JellyBarrage, Ray>>((aimRay, self) =>
                            {
                                if (AllowPrediction(self.characterBody, loopOnly))
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.VagrantMonster.Weapon.JellyBarrage.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                                return aimRay;
                            });
                            error = false;
                        }
                    }

                    if (error)
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate -= (il) =>
                {
                    bool error = true;
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.VagrantMonster.Weapon.JellyBarrage, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.VagrantMonster.Weapon.JellyBarrage.projectilePrefab, targetHurtbox);
                                return newAimRay;
                            }
                            return aimRay;
                        });
                        if (c.TryGotoNext(MoveType.After,
                             x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                            ))
                        {
                            c.Emit(OpCodes.Ldarg_0);
                            c.EmitDelegate<Func<Ray, EntityStates.VagrantMonster.Weapon.JellyBarrage, Ray>>((aimRay, self) =>
                            {
                                if (AllowPrediction(self.characterBody, loopOnly))
                                {
                                    HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                    Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.VagrantMonster.Weapon.JellyBarrage.projectilePrefab, targetHurtbox);
                                    return newAimRay;
                                }
                                return aimRay;
                            });
                            error = false;
                        }
                    }

                    if (error)
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate IL Hook failed");
                    }
                };
            }
        }
        public class VoidJailer
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                On.EntityStates.VoidJailer.Weapon.Fire.ModifyProjectileAimRay += (orig, self, aimRay) =>
                {
                    if (AllowPrediction(self.characterBody, loopOnly))
                    {
                        HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                        Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                        return orig(self, newAimRay);
                    }
                    return orig(self, aimRay);
                };
            }
            public static void UnInit()
            {
                On.EntityStates.VoidJailer.Weapon.Fire.ModifyProjectileAimRay -= (orig, self, aimRay) =>
                {
                    if (AllowPrediction(self.characterBody, loopOnly))
                    {
                        HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                        Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, self.projectilePrefab, targetHurtbox);
                        return orig(self, newAimRay);
                    }
                    return orig(self, aimRay);
                };
            }
        }
        public class Vulture
        {
            public static bool enabled = true;
            public static bool loopOnly = false;

            public static void Init()
            {
                if (!enabled) return;


                IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.Vulture.Weapon.FireWindblade, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.Vulture.Weapon.FireWindblade.projectilePrefab, targetHurtbox);
                                //Feed it the projectile prefab in case a mod is changing the speed.
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.Vulture.Weapon.FireWindblade.OnEnter IL Hook failed");
                    }
                };
            }
            public static void UnInit()
            {
                IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter -= (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(MoveType.After,
                         x => x.MatchCall<EntityStates.BaseState>("GetAimRay")
                        ))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<Ray, EntityStates.Vulture.Weapon.FireWindblade, Ray>>((aimRay, self) =>
                        {
                            if (AllowPrediction(self.characterBody, loopOnly))
                            {
                                HurtBox targetHurtbox = GetMasterAITargetHurtbox(self.characterBody.master);
                                Ray newAimRay = PredictAimrayPS(aimRay, self.GetTeam(), 45f, EntityStates.Vulture.Weapon.FireWindblade.projectilePrefab, targetHurtbox);
                                //Feed it the projectile prefab in case a mod is changing the speed.
                                return newAimRay;
                            }
                            return aimRay;
                        });
                    }
                    else
                    {
                        ScrapyardLog.Debug("Whirlwind: EntityStates.Vulture.Weapon.FireWindblade.OnEnter IL Hook failed");
                    }
                };
            }
        }
        #endregion
        public static bool AllowPrediction(CharacterBody body, bool loopOnly)
        {
            if (prediction && Run.instance)
            {
                DifficultyDef df = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
                if (df != null && !df.countsAsHardMode)
                {
                    return false;
                }
            }

            //Check this first, since it should skip other checks.
            if (body && body.isChampion) return true;

            if (loopOnly && Run.instance && Run.instance.stageClearCount < 5) return false;
            if (body)
            {
                if (body.isPlayerControlled) return false;
            }

            return true;
        }

        public static Ray PredictAimray(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle, float projectileSpeed, HurtBox targetHurtBox)
        {
            bool hasHurtbox = false;
            float percentageSlow = 1f;
            if (targetHurtBox == null)
            {
                targetHurtBox = AcquireTarget(aimRay, attackerTeam, maxTargetAngle);
            }

            hasHurtbox = targetHurtBox && targetHurtBox.healthComponent && targetHurtBox.healthComponent.body && targetHurtBox.healthComponent.body.characterMotor;

            if (hasHurtbox && projectileSpeed > 0f)
            {
                CharacterBody targetBody = targetHurtBox.healthComponent.body;
                Vector3 targetPosition = targetHurtBox.transform.position;

                //Velocity shows up as 0 for clients due to not having authority over the CharacterMotor
                Vector3 targetVelocity = targetBody.characterMotor.velocity;
                if (!targetBody.hasAuthority)
                {
                    //Less accurate, but it works online.
                    targetVelocity = (targetBody.transform.position - targetBody.previousPosition) / Time.fixedDeltaTime;
                }

                if (targetVelocity.sqrMagnitude > 0f && !(targetBody && targetBody.hasCloakBuff))   //Dont bother predicting stationary targets
                {
                    if (targetHurtBox.collider.gameObject.TryGetComponent<CapsuleCollider>(out CapsuleCollider var))
                    {
                        percentageSlow = Mathf.Clamp(2.03f / (var.radius + var.height), 0.4f, 1f);
                    }
                    else if (targetHurtBox.collider.gameObject.TryGetComponent<SphereCollider>(out SphereCollider var2))
                    {
                        percentageSlow = Mathf.Clamp(2.03f / var2.radius, 0.4f, 1f);
                    }
                    int random = UnityEngine.Random.Range(1, 5);

                    if (random == 1)
                    {
                        if (percentageSlow == 1f) percentageSlow *= UnityEngine.Random.Range(1f, 1.25f);

                        projectileSpeed *= percentageSlow;

                    }
                    else if(random == 2)
                    {
                        if (percentageSlow == 1f) percentageSlow *= UnityEngine.Random.Range(1f, 1.25f);

                        projectileSpeed /= percentageSlow;
                    }
                    else projectileSpeed *= percentageSlow;
                    //A very simplified way of estimating, won't be 100% accurate.
                    Vector3 currentDistance = targetPosition - aimRay.origin;
                    float timeToImpact = currentDistance.magnitude / projectileSpeed;

                    //Vertical movenent isn't predicted well by this, so just use the target's current Y
                    Vector3 lateralVelocity = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
                    Vector3 futurePosition = targetPosition + lateralVelocity * timeToImpact;

                    //Only attempt prediction if player is jumping upwards.
                    //Predicting downwards movement leads to groundshots.
                    if (targetBody.characterMotor && !targetBody.characterMotor.isGrounded && targetVelocity.y > 0f)
                    {
                        //point + vt + 0.5at^2
                        float futureY = targetPosition.y + targetVelocity.y * timeToImpact;
                        futureY += 0.5f * Physics.gravity.y * timeToImpact * timeToImpact;
                        futurePosition.y = futureY;
                    }

                    Ray newAimray = new Ray
                    {
                        origin = aimRay.origin,
                        direction = (futurePosition - aimRay.origin).normalized
                    };

                    float angleBetweenVectors = Vector3.Angle(aimRay.direction, newAimray.direction);
                    if (angleBetweenVectors <= maxTargetAngle)
                    {
                        return newAimray;
                    }
                }
            }

            return aimRay;
        }

        public static Ray PredictAimrayPS(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle, GameObject projectilePrefab, HurtBox targetHurtBox)
        {
            float speed = -1f;
            if (projectilePrefab)
            {
                ProjectileSimple ps = projectilePrefab.GetComponent<ProjectileSimple>();
                if (ps)
                {
                    speed = ps.desiredForwardSpeed;
                }
            }

            if (speed <= 0f)
            {
                ScrapyardLog.Debug("Whirlwind: Could not get speed of ProjectileSimple.");
                return aimRay;
            }

            return speed > 0f ? PredictAimray(aimRay, attackerTeam, maxTargetAngle, speed, targetHurtBox) : aimRay;
        }

        public static Ray PredictAimrayPCC(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle, GameObject projectilePrefab, HurtBox targetHurtBox)
        {
            float speed = -1f;
            if (projectilePrefab)
            {
                ProjectileCharacterController pcc = projectilePrefab.GetComponent<ProjectileCharacterController>();
                if (pcc)
                {
                    speed = pcc.velocity;
                }
            }

            if (speed <= 0f)
            {
                ScrapyardLog.Debug("Whirlwind: Could not get speed of ProjectileCharacterController.");
                return aimRay;
            }

            return PredictAimray(aimRay, attackerTeam, maxTargetAngle, speed, targetHurtBox);
        }

        public static HurtBox AcquireTarget(Ray aimRay, TeamIndex attackerTeam, float maxTargetAngle)
        {
            BullseyeSearch search = new BullseyeSearch();

            search.teamMaskFilter = TeamMask.allButNeutral;
            search.teamMaskFilter.RemoveTeam(attackerTeam);

            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.sortMode = BullseyeSearch.SortMode.Angle;
            search.maxDistanceFilter = 200f;
            search.maxAngleFilter = maxTargetAngle;
            search.searchDirection = aimRay.direction;
            search.RefreshCandidates();

            HurtBox targetHurtBox = search.GetResults().FirstOrDefault<HurtBox>();

            return targetHurtBox;
        }

        public static HurtBox GetMasterAITargetHurtbox(CharacterMaster cm)
        {
            if (cm && cm.aiComponents.Length > 0)
            {
                foreach (BaseAI ai in cm.aiComponents)
                {
                    if (ai.currentEnemy != null && ai.currentEnemy.bestHurtBox != null)
                    {
                        return ai.currentEnemy.bestHurtBox;
                    }
                }
            }
            return null;
        }
    }
}