using FortunesFromTheScrapyard.Survivors.Cloaker;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Cloaker.Weapon
{
    public class CloakerShoot : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public const float PrimaryDamageCoefficient = 1.3f;
        public float damageCoefficient = PrimaryDamageCoefficient;
        public bool charged;
        internal float? cloakedDamage;
        private const float BaseDuration = 0.4f;
        private float duration;
        private int step;
        private bool crit;
        private string muzzle;
        public void SetStep(int value) => step = value;

        public override void OnEnter()
        {
            base.OnEnter();
            if (charged) damageStat = cloakedDamage ?? damageStat;
            CloakerController controller = GetComponent<CloakerController>();
            controller.BreakStealth();
            duration = BaseDuration / attackSpeedStat;
            characterBody.isSprinting = false;
            characterBody.SetAimTimer(2f);
            muzzle = controller.isAkimbo && step % 2 != 0 ? "MuzzleLeft" : "MuzzleRight";
            if (isAuthority) crit = RollCrit();
            string animation = charged ? "ShootSecondary"
                : controller.isAkimbo ? (muzzle == "MuzzleLeft" ? "ShootDual2" : "ShootDual1") : "Shoot";
            PlayCrossfade("Gesture, Override", controller.GetAnimationStateName("Gesture, Override", animation),
                charged ? "Secondary.playbackRate" : "Primary.playbackRate", duration, duration * 0.05f);
            EffectManager.SimpleMuzzleFlash(CloakerAssets.PistolMuzzle, gameObject, muzzle, false);
            Util.PlaySound(charged ? CloakerAssets.ChargedShotSound : CloakerAssets.ShotSound, gameObject);
            if (isAuthority) Fire();
            characterBody.AddSpreadBloom(1.25f);
        }

        private void Fire()
        {
            Ray aimRay = GetAimRay();
            AddRecoil(-1f, -1f, -1f, 1f);
            BulletAttack bullet = new BulletAttack
            {
                bulletCount = 1,
                origin = aimRay.origin,
                aimVector = aimRay.direction,
                damage = damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                maxDistance = 2000f,
                force = 200f,
                hitMask = LayerIndex.CommonMasks.bullet,
                stopperMask = LayerIndex.CommonMasks.bullet,
                minSpread = 0f,
                maxSpread = characterBody.spreadBloomAngle * 2f,
                isCrit = crit,
                owner = gameObject,
                muzzleName = muzzle,
                smartCollision = true,
                procChainMask = default,
                procCoefficient = 1f,
                radius = 0.75f,
                sniper = false,
                tracerEffectPrefab = crit ? CloakerAssets.CritTracer : charged ? CloakerAssets.RailTracer : CloakerAssets.GoldTracer,
                hitEffectPrefab = charged ? CloakerAssets.RailImpact : CloakerAssets.PistolImpact,
                spreadPitchScale = 1f,
                spreadYawScale = 1f,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
            };
            if (charged) bullet.AddModdedDamageType(CloakerContent.CloakerChargedDamageType);
            bullet.Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
        public override void Reset()
        {
            base.Reset();
            cloakedDamage = null;
            damageCoefficient = PrimaryDamageCoefficient;
            charged = false;
            step = 0;
            crit = false;
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(damageCoefficient);
            writer.Write(charged);
            writer.Write(step);
            writer.Write(crit);
            writer.Write(cloakedDamage.HasValue);
            writer.Write(cloakedDamage.GetValueOrDefault());
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            damageCoefficient = reader.ReadSingle();
            charged = reader.ReadBoolean();
            step = reader.ReadInt32();
            crit = reader.ReadBoolean();
            bool hasCloakedDamage = reader.ReadBoolean();
            float damage = reader.ReadSingle();
            cloakedDamage = hasCloakedDamage ? (float?)damage : null;
        }
    }
}
