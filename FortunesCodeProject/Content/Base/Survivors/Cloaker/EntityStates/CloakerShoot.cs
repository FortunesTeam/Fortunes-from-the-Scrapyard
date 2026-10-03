using EntityStates;
using FortunesFromTheScrapyard;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;

namespace EntityStates.Cloaker.Weapon
{
    public class CloakerShoot : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 20f, configDescOverride = "Primary bullet damage coefficient. 2.8 deals 280% damage.")]
        [FormatToken(CloakerContent.PRIMARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float PrimaryDamageCoefficient = 2.8f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0.01f, 5f, configDescOverride = "Shot duration in seconds before attack speed scaling.")]
        public static float BaseDuration = 0.4f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 1f, 5000f, configDescOverride = "Maximum bullet travel distance in meters, for primary and charged shots.")]
        public static float MaxDistance = 2000f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 5000f, configDescOverride = "Bullet knockback force, for primary and charged shots.")]
        public static float Force = 200f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 5f, configDescOverride = "Bullet proc coefficient, for primary and charged shots.")]
        public static float ProcCoefficient = 1f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 5f, configDescOverride = "Bullet hit radius in meters, for primary and charged shots.")]
        public static float Radius = 0.75f;

        public float damageCoefficient = PrimaryDamageCoefficient;
        public bool charged;
        internal float? cloakedDamage;
        private float duration;
        private int step;
        private bool crit;
        private string muzzle;
        public void SetStep(int value) => step = value;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!charged) damageCoefficient = PrimaryDamageCoefficient;
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
                charged ? "Secondary.playbackRate" : "Primary.playbackRate", duration, duration * 0.15f);
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
                maxDistance = MaxDistance,
                force = Force,
                hitMask = LayerIndex.CommonMasks.bullet,
                stopperMask = LayerIndex.CommonMasks.bullet,
                minSpread = 0f,
                maxSpread = characterBody.spreadBloomAngle * 2f,
                isCrit = crit,
                owner = gameObject,
                muzzleName = muzzle,
                smartCollision = true,
                procChainMask = default,
                procCoefficient = ProcCoefficient,
                radius = Radius,
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
