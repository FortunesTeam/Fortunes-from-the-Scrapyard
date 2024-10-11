using UnityEngine;
using RoR2;
using RoR2.Projectile;
using System;
using FortunesFromTheScrapyard.Survivors.Skater;

namespace EntityStates.Skater
{
    public class Bubble : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 3f);
            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile(SkaterSurvivor.bubbleProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * 2f, 0f, RollCrit());
                outer.SetNextStateToMain();
            }
        }
    }
}
