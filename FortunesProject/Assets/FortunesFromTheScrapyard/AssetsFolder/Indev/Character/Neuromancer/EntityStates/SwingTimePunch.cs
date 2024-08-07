using EntityStates;


using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class SwingTimePunch : BaseNeuromancerMeleeState
    {
        public static float selfKnockback;

        public float charge;

        public float minLungeSpeed;

        public float maxLungeSpeed;

        public float minPunchForce;

        public float maxPunchForce;

        public float minDuration;

        public float maxDuration;

        public static bool disableAirControlUntilCollision;

        public static float speedCoefficientOnExit = 0.5f;

        protected Vector3 punchVelocity;

        public float punchSpeed { get; private set; }

        private bool hasHit;

        private GameObject swingEffectInstance;

        public override void OnEnter()
        {
            damageCoefficient = 8f * charge;
            minLungeSpeed = 26f;
            maxLungeSpeed = 40f;
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
            hitEffectPrefab = Neuromancer.timePunchHitEffect;
            procCoefficient = 1f;
            pushForce = 1200f;
            bonusForce = new Vector3(0f, 0f, 2400);
            hitStopDuration = 1.5f;
            swingEffectPrefab = Neuromancer.timePunchSwingEffect;
            muzzleString = "HandL";
            hitHopVelocity = 4f;
            swingSoundString = "Play_loader_shift_release";
            impactSound = Neuromancer.timePunchHitSoundDef.index;
            selfKnockback = 2000f;
            moddedDamageTypeHolder.Add(Neuromancer.DelayedUtility);

            base.OnEnter();

            if (base.isAuthority)
            {
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.disableAirControlUntilCollision |= disableAirControlUntilCollision;
                punchVelocity = CalculateLungeVelocity(base.characterMotor.velocity, GetAimRay().direction, charge, minLungeSpeed, maxLungeSpeed);
                base.characterMotor.velocity = punchVelocity;
                base.characterDirection.forward = base.characterMotor.velocity.normalized;
                punchSpeed = base.characterMotor.velocity.magnitude;
            }
        }

        protected override float CalcDuration()
        {
            return Mathf.Lerp(minDuration, maxDuration, charge);
        }
        protected override void PlaySwingEffect()
        {
            Util.PlaySound(this.swingSoundString, this.gameObject);
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.swingEffectPrefab, muzzleTransform);
                }
            }
        }
        protected override void PlayAttackAnimation()
        {
            PlayAnimation("FullBody, Override", "ChargePunch", "ChargePunch.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!base.inHitPause)
            {
                base.characterMotor.velocity = punchVelocity;
                base.characterDirection.forward = punchVelocity;
                base.characterBody.isSprinting = true;
            }
        }

        protected override void FireAttack()
        {
            attack.forceVector = base.characterMotor.velocity + GetAimRay().direction * Mathf.Lerp(minPunchForce, maxPunchForce, charge);
            if (base.fixedAge + Time.fixedDeltaTime >= duration)
            {
                HitBoxGroup hitBoxGroup = FindHitBoxGroup("TimePunch");
                if (hitBoxGroup)
                {
                    hitboxGroupName = hitBoxGroup.groupName;
                    attack.hitBoxGroup = hitBoxGroup;
                }
            }
            base.FireAttack();
        }

        protected override void OnHitEnemyAuthority()
        {
            if(!hasHit)
            {
                base.OnHitEnemyAuthority();
                hasHit = true;
            }
        }

        protected override void ApplyHitstop()
        {
            base.ApplyHitstop();
            outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();

            if (this.swingEffectInstance) EntityState.Destroy(this.swingEffectInstance);

            base.characterMotor.velocity *= speedCoefficientOnExit;
            if (base.isAuthority && hasHit && base.healthComponent)
            {
                Vector3 vector = punchVelocity;
                vector.y = Mathf.Min(vector.y, 0f);
                vector = vector.normalized;
                vector *= 0f - selfKnockback;
                if (base.characterMotor)
                {
                    base.characterMotor.ApplyForce(vector, alwaysApply: true);
                }
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

