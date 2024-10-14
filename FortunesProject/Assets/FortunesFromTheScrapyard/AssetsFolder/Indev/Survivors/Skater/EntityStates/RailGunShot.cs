using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using FortunesFromTheScrapyard;
using UnityEngine.AddressableAssets;
using FortunesFromTheScrapyard.Survivors.Skater;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using FortunesFromTheScrapyard.Survivors.Duke;
using R2API;
using UnityEngine.Networking;

namespace EntityStates.Skater
{
    public class Salvo : BaseSkillState
    {
        public static float baseProcCoefficient = 1f;
        public static float baseShootDuration = 0.25f;
        public static float baseDuration = 1.25f;
        public static float baseForce = 600f;
        public static int bulletCount = 1;
        public static float baseBulletSpread = 0f;
        public static float baseBulletRadius = 0.4f;
        public static float baseBulletRecoil = 2f;
        public static float baseBulletRange = 999f;
        public static float baseSelfForce = 750f;

        public static GameObject tracerEffectPrefab = DukeSurvivor.dukeTracerCrit;

        private float windupDuration;
        private float duration;
        private string muzzleString;
        private bool isCrit;
        private float recoil;
        private bool gaveQuickReset;
        private float damageCoefficient;
        private float procCoefficient;
        private float force;
        private float bulletSpread;
        private float bulletRadius;
        private float bulletRecoil;
        private float bulletRange;
        private float selfForce;
        private bool hasFired;
        private DamageType damageType = DamageType.Generic;
        protected GameObject tracerPrefab = tracerEffectPrefab;
        public string shootSoundString = "Play_railgunner_R_fire";
        public BulletAttack.FalloffModel falloff = BulletAttack.FalloffModel.DefaultBullet;

        public override void OnEnter()
        {
            this.damageCoefficient = SkaterSurvivor.baseRailgunDamageCoefficient;
            this.procCoefficient = baseProcCoefficient;
            this.force = baseForce;
            this.bulletSpread = baseBulletSpread;
            this.bulletRadius = baseBulletRadius;
            this.bulletRecoil = baseBulletRecoil;
            this.bulletRange = baseBulletRange;
            this.selfForce = baseSelfForce;

            base.OnEnter();
            this.windupDuration = baseShootDuration / this.attackSpeedStat;
            this.duration = baseDuration / this.attackSpeedStat;

            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "GunMuzzle";

            this.isCrit = base.RollCrit();

            this.recoil = bulletRecoil / this.attackSpeedStat;

            tracerPrefab = tracerEffectPrefab;

            PlayAnimations();
        }

        public virtual void PlayAnimations()
        {
            Animator animator = GetModelAnimator();

            if (animator)
            {
                bool isMoving = animator.GetBool("isMoving");
                bool isGrounded = animator.GetBool("isGrounded");
                if (!isMoving && isGrounded)
                {
                    this.PlayCrossfade("FullBody, Override", "EnterShoot", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.15f);
                }
                else //If moving
                {
                    this.PlayCrossfade("Gesture, Additive", "EnterShoot", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.25f);
                }
            }
        }
        public override void OnExit()
        {
            Animator animator = GetModelAnimator();
            if (animator)
            {
                //animator.SetLayerWeight(animator.GetLayerIndex("Gesture, ShootBody"), 0f);
            }
            if (!hasFired)
            {
                this.Fire();
            }
            base.OnExit();
        }

        private void Fire()
        {
            Util.PlaySound(this.shootSoundString, this.gameObject);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                BulletAttack bulletAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = this.damageCoefficient * this.damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
                    falloffModel = this.falloff,
                    maxDistance = bulletRange,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = this.isCrit,
                    owner = base.gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = bulletRadius,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = this.tracerPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    HitEffectNormal = false,
                    trajectoryAimAssistMultiplier = 0.3f
                };

                bulletAttack.minSpread = 0;
                bulletAttack.maxSpread = 0;
                bulletAttack.bulletCount = 1;

                bulletAttack.Fire();

                if (!this.characterMotor.isGrounded) this.characterMotor.ApplyForce(aimRay.direction * -this.selfForce);
            }

            base.characterBody.AddSpreadBloom(2.5f);

            hasFired = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.windupDuration && !hasFired)
            {
                this.Fire();
            }

            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge >= this.duration) return InterruptPriority.Any;
            return InterruptPriority.PrioritySkill;
        }
    }

}
