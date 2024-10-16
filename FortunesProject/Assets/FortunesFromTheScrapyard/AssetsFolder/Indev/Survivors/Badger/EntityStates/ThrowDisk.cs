﻿// using BadgerMod.Modules.BaseStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
// using BadgerMod.Badger.Content;
using R2API;
using EntityStates;
using EntityStates.Badger.Components;
using RoR2.Projectile;
using FortunesFromTheScrapyard.Survivors.Badger;

namespace EntityStates.Badger
{
    public class ThrowDisk : GenericProjectileBaseState
    {
        public static float baseDuration = 0.2f;
        public static float baseDelayDuration = 0.3f * baseDuration;
        public static float diskDamageCoefficent = 4f;
        public GameObject disk = BadgerSurvivor.diskPrefab;
        public BadgerController badgerController;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            badgerController = base.gameObject.GetComponent<BadgerController>();
            base.attackSoundString = "sfx_badger_diskthrow";

            base.baseDuration = baseDuration;
            base.baseDelayBeforeFiringProjectile = baseDelayDuration;

            base.force = 0f;

            base.projectilePitchBonus = -3.5f;

            this.PlayCrossfade("Gesture, Override", "DiskThrow", "Utility.playbackRate", duration * 5f, 0.05f);

            base.OnEnter();
        }

        public override void FireProjectile()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay = this.ModifyProjectileAimRay(aimRay);
                aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, 0f, this.projectilePitchBonus);

                ProjectileManager.instance.FireProjectile(disk, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat * diskDamageCoefficent, this.force, this.RollCrit(), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void PlayAnimation(float duration)
        {
            if (base.GetModelAnimator())
            {
                base.PlayAnimation("Gesture, Override", "SwingCleaver", "Swing.playbackRate", this.duration * 5.5f);
            }
        }
    }
}

