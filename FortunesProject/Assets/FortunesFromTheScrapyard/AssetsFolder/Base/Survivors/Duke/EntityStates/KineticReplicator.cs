using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using FortunesFromTheScrapyard.Survivors.Duke;
using R2API;

namespace EntityStates.Duke
{
    public class KineticReplicator : GenericProjectileBaseState
    {
        public float baseDuration = 0.15f;
        public float baseDelayDuration = 0.05f;
        public GameObject mine = DukeSurvivor.damageShareMine;
        public DukeController dukeController;
        public override void OnEnter()
        {
            dukeController = base.gameObject.GetComponent<DukeController>();
            base.attackSoundString = "sfx_duke_fire_mine";

            base.baseDuration = baseDuration;
            base.baseDelayBeforeFiringProjectile = baseDelayDuration;

            base.damageCoefficient = damageCoefficient;
            base.force = 120f;

            base.projectilePitchBonus = -3.5f;

            base.OnEnter();
        }

        public override void FireProjectile()
        {
            Util.PlaySound(attackSoundString, base.gameObject);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay = this.ModifyProjectileAimRay(aimRay);
                aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, 0f, this.projectilePitchBonus);
                ProjectileManager.instance.FireProjectile(mine, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, 0f, 0f, false, DamageColorIndex.Default, null, -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void PlayAnimation(float duration)
        {
            if (base.GetModelAnimator())
            {
                this.PlayCrossfade("Gesture, Override", "Shoot", "Shoot.playbackRate", this.duration * 1.5f, this.duration * 0.05f);
            }
        }
    }
}
