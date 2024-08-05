using EntityStates;
using EntityStates.BeetleGuardMonster;
using EntityStates.Bell.BellWeapon;
using EntityStates.ClayBoss;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.GravekeeperBoss;
using EntityStates.GreaterWispMonster;
using EntityStates.ImpBossMonster;
using EntityStates.ImpMonster;
using EntityStates.LemurianBruiserMonster;
using EntityStates.LemurianMonster;
using EntityStates.RoboBallBoss.Weapon;
using EntityStates.ScavMonster;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.Vulture.Weapon;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.Networking;
using HarmonyLib;

namespace FortunesFromTheScrapyard
{
    public class Whirlwind : ScrapyardDifficulty
    {
        public override ScrapyardAssetRequest<SerializableDifficultyDef> AssetRequest => ScrapyardAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", ScrapyardBundle.Indev);

        public static SerializableDifficultyDef whirlwindDifficulty;

        internal static bool prediction = true;
        internal static float attackSpeed = 1.5f;
        internal static float moveSpeed = 1.5f;
        internal static float cdr = 0.5f;
        internal static float teleporterRadius = -50f;
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
            On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;

            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;

            On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;
        }

        public override void OnRunStart(Run run)
        {
            if (run.selectedDifficulty == whirlwindDifficulty.DifficultyIndex)
            {
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;

                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

                On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;
                
                IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += (il) => FireSunder_FixedUpdate(new ILCursor(il));
                IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += (il) => ChargeTrioBomb_FixedUpdate(new ILCursor(il));

                // generic type T
                IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) => FireProjectile<GenericProjectileBaseState>(new ILCursor(il));
                IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += (il) => FireProjectile<FireCannons>(new ILCursor(il));
                IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += (il) => FireProjectile<FireEyeBlast>(new ILCursor(il));
                IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += (il) => FireProjectile<FireWindblade>(new ILCursor(il));
                IL.EntityStates.GravekeeperBoss.FireHook.OnEnter += (il) => FireProjectile<FireHook>(new ILCursor(il));
                IL.EntityStates.LemurianMonster.FireFireball.OnEnter += (il) => FireProjectile<FireFireball>(new ILCursor(il));
                IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += (il) => FireProjectile<FireMegaFireball>(new ILCursor(il));
                IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += (il) => FireProjectile<FireEnergyCannon>(new ILCursor(il));
                IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += (il) => FireProjectile<FireBombardment>(new ILCursor(il));
                IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball += (il) => FireProjectile<FireTarball>(new ILCursor(il));
                IL.EntityStates.ImpMonster.FireSpines.FixedUpdate += (il) => FireProjectile<FireSpines>(new ILCursor(il));

                //fire many
                IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate += (il) => FireProjectileGroup<JellyBarrage>(new ILCursor(il));
                IL.EntityStates.ImpBossMonster.FireVoidspikes.FixedUpdate += (il) => FireProjectileGroup<FireVoidspikes>(new ILCursor(il));
                
                foreach (CharacterMaster cm in run.userMasters.Values)
                    if (NetworkServer.active)
                        cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
            }
        }
        
        #region Generic Prediction IL
        private static void EmitPredictAimray(ILCursor c, Type type, string prefabName = "projectilePrefab")
        {
            //this.characterbody
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody)));

            // this.projectilePrefab
            // - or -
            // {TYPE}.{PREFAB_NAME}
            var fieldInfo = AccessTools.Field(type, prefabName);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldarg_0);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldfld, fieldInfo);
            else c.Emit(OpCodes.Ldsfld, fieldInfo);

            // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(PredictionUtils), nameof(PredictionUtils.PredictAimrayNew)));
        }

        private static void FireProjectile<T>(ILCursor c, string prefabName)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T), prefabName);
            else ScrapyardLog.Error("Failed to apply Eclipse 2 Generic BaseState.GetAimRay IL Hook");
        }

        private static void FireProjectile<T>(ILCursor c)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
            else ScrapyardLog.Error("Failed to apply Eclipse 2 Generic BaseState.GetAimRay IL Hook");
        }

        private static void FireProjectileGroup<T>(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                EmitPredictAimray(c, typeof(T));
        }
        #endregion

        private static void FireSunder_FixedUpdate(ILCursor c)
        {
            int loc = 0;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchStloc(out loc)) &&
                c.TryGotoNext(x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                c.Emit(OpCodes.Ldloc, loc);
                EmitPredictAimray(c, typeof(FireSunder));
                c.Emit(OpCodes.Stloc, loc);
            }
            else ScrapyardLog.Error("Failed to apply Eclipse 2 BeetleGuardMonster.FireSunder.FixedUpdate IL Hook");
        }

        private static void ChargeTrioBomb_FixedUpdate(ILCursor c)
        {
            int rayLoc = 0, transformLoc = 0;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchStloc(out rayLoc)) &&
                c.TryGotoNext(x => x.MatchCall<ChargeTrioBomb>(nameof(ChargeTrioBomb.FindTargetChildTransformFromBombIndex))) &&
                c.TryGotoNext(x => x.MatchStloc(out transformLoc)) &&
                c.TryGotoNext(x => x.MatchCall(AccessTools.PropertyGetter(typeof(ProjectileManager), nameof(ProjectileManager.instance)))))
            {
                // set origin
                c.Emit(OpCodes.Ldloc, rayLoc);
                c.Emit(OpCodes.Ldloc, transformLoc);
                c.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Transform), nameof(Transform.position)));
                c.Emit(OpCodes.Call, AccessTools.PropertySetter(typeof(Ray), nameof(Ray.origin)));

                // call prediction utils
                c.Emit(OpCodes.Ldloc, rayLoc);
                EmitPredictAimray(c, typeof(ChargeTrioBomb), "bombProjectilePrefab");
                c.Emit(OpCodes.Stloc, rayLoc);
            }
            else ScrapyardLog.Error("AccurateEnemies: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
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

        internal class PredictionUtils
        {
            private static bool ShouldPredict(CharacterBody body)
            {
                return body && body.master && body.teamComponent.teamIndex != TeamIndex.Player &&
                    Run.instance && Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse2 &&
                    (Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse5 || body.isElite);
            }

            public static Ray PredictAimrayNew(Ray aimRay, CharacterBody body, GameObject projectilePrefab)
            {
                if (!ShouldPredict(body) || !projectilePrefab)
                    return aimRay;

                // lil bit of wiggle room cuz floats are fun
                float zero = 0.1f * Time.fixedDeltaTime;
                float projectileSpeed = 0f;
                if (projectilePrefab.TryGetComponent<ProjectileSimple>(out var ps))
                {
                    if (body.teamComponent.teamIndex != TeamIndex.Player && ps.rigidbody && !ps.rigidbody.useGravity)
                        projectileSpeed = ScrapyardMain.GetProjectileSimpleModifiers(ps.desiredForwardSpeed);
                    else
                        projectileSpeed = ps.desiredForwardSpeed;
                }

                if (projectilePrefab.TryGetComponent<ProjectileCharacterController>(out var pcc))
                    projectileSpeed = Mathf.Max(projectileSpeed, pcc.velocity);

                if (projectileSpeed <= zero)
                    ScrapyardLog.Warning($"Projectile speed is {projectileSpeed}? you fucked up man. ");

                if (projectileSpeed > zero && GetTargetHurtbox(body, out var targetBody))
                {
                    //Velocity shows up as 0 for clients due to not having authority over the CharacterMotor
                    //Less accurate, but it works online.
                    var targetVelocity = (targetBody.transform.position - targetBody.previousPosition) / Time.fixedDeltaTime;
                    var motor = targetBody.characterMotor;

                    // compare the two options since big number = better number of course
                    if (motor && targetVelocity.sqrMagnitude < motor.velocity.sqrMagnitude)
                        targetVelocity = motor.velocity;

                    if (targetVelocity.sqrMagnitude > zero * zero) //Dont bother predicting stationary targets
                    {
                        return GetRay(aimRay, projectileSpeed, targetBody.transform.position, targetVelocity);
                    }
                }

                return aimRay;
            }

            private static Ray GetRay(Ray aimRay, float v, Vector3 y, Vector3 dy)
            {
                // dont question it man im so bad at math
                // but its really fucking fast and really fucking accurate
                // might want to integrate acceleration at some point to
                // cut down on overshooting decelerating targets
                // edit: never fuckign mind i hate math im not gonna solve a fucking quartic equation what the hell
                // https://gamedev.stackexchange.com/questions/77749/predicted-target-location
                //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
                var yx = y - aimRay.origin;

                var a = (v * v) - Vector3.Dot(dy, dy);
                var b = -2 * Vector3.Dot(dy, yx);
                var c = -1 * Vector3.Dot(yx, yx);

                var d = (b * b) - (4 * a * c);
                if (d > 0)
                {
                    d = Mathf.Sqrt(d);
                    var t1 = (-b + d) / (2 * a);
                    var t2 = (-b - d) / (2 * a);
                    var t = Mathf.Max(t1, t2);
                    if (t > 0)
                    {
                        var newA = (dy * t + yx) / t;
                        aimRay = new Ray(aimRay.origin, newA.normalized);
                    }
                }
                return aimRay;
            }

            private static bool GetTargetHurtbox(CharacterBody body, out CharacterBody target)
            {
                var aiComponents = body.master.aiComponents;
                for (int i = 0; i < aiComponents.Length; i++)
                {
                    var ai = aiComponents[i];
                    if (ai && ai.hasAimTarget)
                    {
                        var aimTarget = ai.skillDriverEvaluation.aimTarget;
                        if (aimTarget.characterBody && aimTarget.healthComponent)
                        {
                            target = aimTarget.characterBody;
                            return true;
                        }
                    }
                }
                target = null;
                return false;
            }
        }
    }
}