using System;
using System.Collections.Generic;
using System.Text;
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
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using MSU.Config;

namespace FortunesFromTheScrapyard
{
    public class PredictionHooks
    {
        public static void Init()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += (orig, self, maxDist, _, filterByLoS) => orig(self, maxDist, true/*360*/, filterByLoS);

            // special cases
            IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += (il) => FireSunder_FixedUpdate(new(il));
            IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += (il) => ChargeTrioBomb_FixedUpdate(new(il));

            // generic type T
            IL.EntityStates.GenericProjectileBaseState.FireProjectile += (il) => FireProjectile<GenericProjectileBaseState>(new(il));
            IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += (il) => FireProjectile<FireCannons>(new(il));
            IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += (il) => FireProjectile<FireEyeBlast>(new(il));
            IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += (il) => FireProjectile<FireWindblade>(new(il));
            IL.EntityStates.GravekeeperBoss.FireHook.OnEnter += (il) => FireProjectile<FireHook>(new(il));
            IL.EntityStates.LemurianMonster.FireFireball.OnEnter += (il) => FireProjectile<FireFireball>(new(il));
            IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += (il) => FireProjectile<FireMegaFireball>(new(il));
            IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += (il) => FireProjectile<FireEnergyCannon>(new(il));
            IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += (il) => FireProjectile<FireBombardment>(new(il));
            IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball += (il) => FireProjectile<FireTarball>(new(il));
            IL.EntityStates.ImpMonster.FireSpines.FixedUpdate += (il) => FireProjectile<FireSpines>(new(il));

            //fire many
            IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate += (il) => FireProjectileGroup<JellyBarrage>(new(il));
            IL.EntityStates.ImpBossMonster.FireVoidspikes.FixedUpdate += (il) => FireProjectileGroup<FireVoidspikes>(new(il));
        }

        public static void UnInit()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox -= (orig, self, maxDist, _, filterByLoS) => orig(self, maxDist, true/*360*/, filterByLoS);

            // special cases
            IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate -= (il) => FireSunder_FixedUpdate(new(il));
            IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate -= (il) => ChargeTrioBomb_FixedUpdate(new(il));

            // generic type T
            IL.EntityStates.GenericProjectileBaseState.FireProjectile -= (il) => FireProjectile<GenericProjectileBaseState>(new(il));
            IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter -= (il) => FireProjectile<FireCannons>(new(il));
            IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate -= (il) => FireProjectile<FireEyeBlast>(new(il));
            IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter -= (il) => FireProjectile<FireWindblade>(new(il));
            IL.EntityStates.GravekeeperBoss.FireHook.OnEnter -= (il) => FireProjectile<FireHook>(new(il));
            IL.EntityStates.LemurianMonster.FireFireball.OnEnter -= (il) => FireProjectile<FireFireball>(new(il));
            IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate -= (il) => FireProjectile<FireMegaFireball>(new(il));
            IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter -= (il) => FireProjectile<FireEnergyCannon>(new(il));
            IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade -= (il) => FireProjectile<FireBombardment>(new(il));
            IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball -= (il) => FireProjectile<FireTarball>(new(il));
            IL.EntityStates.ImpMonster.FireSpines.FixedUpdate -= (il) => FireProjectile<FireSpines>(new(il));

            //fire many
            IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FixedUpdate -= (il) => FireProjectileGroup<JellyBarrage>(new(il));
            IL.EntityStates.ImpBossMonster.FireVoidspikes.FixedUpdate -= (il) => FireProjectileGroup<FireVoidspikes>(new(il));
        }

        #region Generics
        private static void FireProjectile<T>(ILCursor c, string prefabName)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                c.EmitPredictAimray<T>(prefabName);
            else ScrapyardLog.Error("AccurateEnemies: Generic OnEnter IL Hook failed ");
        }

        private static void FireProjectile<T>(ILCursor c)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                c.EmitPredictAimray<T>();
            else ScrapyardLog.Error("AccurateEnemies: Generic OnEnter IL Hook failed ");
        }

        private static void FireProjectileGroup<T>(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))))
                c.EmitPredictAimray<T>();
        }
        #endregion

        private static void FireSunder_FixedUpdate(ILCursor c)
        {
            int loc = -1;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchStloc(out loc)) &&
                c.TryGotoNext(x => x.MatchCall<ProjectileManager>(nameof(ProjectileManager.instance))))
            {
                c.Emit(OpCodes.Ldloc, loc);
                c.EmitPredictAimray<FireSunder>();
                c.Emit(OpCodes.Stloc, loc);
            }
            else ScrapyardLog.Error("AccurateEnemies: EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate IL Hook failed");
        }

        private static void ChargeTrioBomb_FixedUpdate(ILCursor c)
        {
            int rayLoc = -1, transformLoc = -1;

            if (c.TryGotoNext(x => x.MatchCall<BaseState>(nameof(BaseState.GetAimRay))) &&
                c.TryGotoNext(x => x.MatchStloc(out rayLoc)) &&
                c.TryGotoNext(x => x.MatchCall<ChargeTrioBomb>(nameof(ChargeTrioBomb.FindTargetChildTransformFromBombIndex))) &&
                c.TryGotoNext(x => x.MatchStloc(out transformLoc)) &&
                c.TryGotoNext(x => x.MatchCall<ProjectileManager>(nameof(ProjectileManager.instance))))
            {
                c.Emit(OpCodes.Ldloc, rayLoc);
                c.Emit(OpCodes.Ldloc, transformLoc);

                //this.characterbody
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Call, typeof(ChargeTrioBomb).GetProperty("get_characterBody"));

                // prefab
                c.Emit(OpCodes.Ldsfld, typeof(ChargeTrioBomb).GetField("bombProjectilePrefab"));

                // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
                c.EmitDelegate<Func<Ray, Transform, CharacterBody, GameObject, Ray>>((aimRay, transform, body, prefab) =>
                {
                    aimRay.origin = transform.position;
                    return PredictionUtils.PredictAimray(aimRay, body, prefab);
                });
                c.Emit(OpCodes.Stloc, rayLoc);
            }
            else ScrapyardLog.Error("AccurateEnemies: EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate IL Hook failed");
        }
    }
}
