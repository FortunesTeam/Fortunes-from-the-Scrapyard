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

namespace EntityStates.Skater
{
    public class PewPew : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static GameObject muzzleEffectPrefab = SkaterSurvivor.skaterMuzzleFlash;

        public static GameObject hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SpiritPunchOmniImpactVFX.prefab").WaitForCompletion();

        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBanditPistol.prefab").WaitForCompletion();

        public static float damageCoefficient = 0.75f;

        public static float force = 200f;

        public static float baseDuration = 0.06f;

        public static string FireSmgSoundString = "Play_seeker_skill1_fire";

        public static float recoilAmplitude = 0.7f;

        public static float spreadBloomValue = 0.15f;

        public static float trajectoryAimAssistMultiplier = 0.75f;

        private int smg;

        private Ray aimRay;

        private float duration;

        private static int FireSmgLeftStateHash = Animator.StringToHash("FireSmg, Left");

        private static int FireSmgRightStateHash = Animator.StringToHash("FireSmg, Right");

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            smg = i;
        }

        private void FireBullet(string targetMuzzle)
        {
            Util.PlaySound(FireSmgSoundString, base.gameObject);
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, transmit: false);
            }
            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
                bulletAttack.damage = damageCoefficient * damageStat;
                bulletAttack.force = force;
                bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                bulletAttack.muzzleName = targetMuzzle;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
                bulletAttack.procCoefficient = 0.7f;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                bulletAttack.Fire();
            }
            base.characterBody.AddSpreadBloom(spreadBloomValue);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            aimRay = GetAimRay();
            StartAimMode(aimRay, 3f);
            if (smg % 2 == 0)
            {
                //PlayAnimation("Gesture Additive, Left", FireSmgLeftStateHash);
                FireBullet("LeftGunMuzzle");
            }
            else
            {
                //PlayAnimation("Gesture Additive, Right", FireSmgRightStateHash);
                FireBullet("RightGunMuzzle");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

}
