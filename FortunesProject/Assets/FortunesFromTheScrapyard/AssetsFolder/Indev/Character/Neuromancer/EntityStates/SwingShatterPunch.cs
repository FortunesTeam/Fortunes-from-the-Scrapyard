using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Neuromancer;

namespace EntityStates.Neuromancer
{
    public class SwingShatterPunch : BaseNeuromancerMeleeState
    {
        public float timePunchDamageCoefficient;

        public float charge;

        public float minPunchForce;

        public float maxPunchForce;

        public float minDuration;

        public float maxDuration;

        private float recoilAmplitudeY;

        private float recoilAmplitudeX;

        private bool hasFired;

        private GameObject swingEffectInstance;

        public override void OnEnter()
        {
            damageCoefficient = 8f;
            minPunchForce = 5000f;
            maxPunchForce = 5000f;
            minDuration = 0.3f;
            maxDuration = 0.3f;
            baseDuration = 0.3f;
            attackStartPercentTime = 0f;
            attackEndPercentTime = 1f;
            earlyExitPercentTime = 1f;
            attackRecoil = 2f / attackSpeedStat;
            damageType = DamageType.Stun1s;
            hitboxGroupName = "TimePunch";
            hitEffectPrefab = NeuromancerSurvivor.timePunchHitEffect;
            procCoefficient = 1f;
            pushForce = 0f;
            bonusForce = Vector3.zero;
            hitStopDuration = 0.25f;
            swingEffectPrefab = NeuromancerSurvivor.timePunchSwingEffect;
            muzzleString = "HandL";
            hitHopVelocity = 4f;
            swingSoundString = "Play_loader_shift_release";
            impactSound = NeuromancerSurvivor.timePunchHitSoundDef.index;
            recoilAmplitudeY = 6f;
            recoilAmplitudeX = 1f;

            base.OnEnter();
        }

        protected override float CalcDuration()
        {
            return Mathf.Lerp(minDuration, maxDuration, charge);
        }
        protected override void PlaySwingEffect()
        {
        }
        protected override void PlayAttackAnimation()
        {
            PlayAnimation("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void FireAttack()
        {
            if(!hasFired)
            {
                hasFired = true;
                Ray aimRay = GetAimRay();

                AddRecoil(-1f * recoilAmplitudeY, -1.5f * recoilAmplitudeY, -1f * recoilAmplitudeX, 1f * recoilAmplitudeX);

                if (base.isAuthority)
                {
                    BulletAttack bulletAttack = new BulletAttack();
                    bulletAttack.owner = base.gameObject;
                    bulletAttack.weapon = base.gameObject;
                    bulletAttack.origin = aimRay.origin;
                    bulletAttack.aimVector = aimRay.direction;
                    bulletAttack.muzzleName = "HandL";
                    bulletAttack.maxDistance = 9999f;
                    bulletAttack.minSpread = 0f;
                    bulletAttack.maxSpread = 0f;
                    bulletAttack.radius = Mathf.Lerp(5f, 15f, charge);
                    bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                    bulletAttack.smartCollision = true;
                    bulletAttack.stopperMask = LayerIndex.world.mask;
                    bulletAttack.hitMask = (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask;
                    bulletAttack.damage = Mathf.Lerp(1f, 8f, charge) * damageStat;
                    bulletAttack.procCoefficient = 1f;
                    bulletAttack.force = 4000f;
                    bulletAttack.isCrit = RollCrit();
                    bulletAttack.hitEffectPrefab = hitEffectPrefab;
                    bulletAttack.tracerEffectPrefab = NeuromancerSurvivor.punchTracer;
                    bulletAttack.AddModdedDamageType(NeuromancerSurvivor.AltUtilityFreeze);
                    bulletAttack.Fire();
                }
            }
        }

        protected override void OnHitEnemyAuthority()
        {
                base.OnHitEnemyAuthority();
        }

        protected override void ApplyHitstop()
        {
            base.ApplyHitstop();
            outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();

            if (swingEffectInstance)
            {
                Destroy(swingEffectInstance);
            }
        }

        public static Vector3 CalculateLungeVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLungeSpeed, float maxLungeSpeed)
        {
            currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
            return currentVelocity + aimDirection * Mathf.Lerp(minLungeSpeed, maxLungeSpeed, charge);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}

