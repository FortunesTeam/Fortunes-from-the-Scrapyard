using UnityEngine;
using EntityStates;
// using BadgerMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using EntityStates.Badger;
using static R2API.DamageAPI;
using UnityEngine.Networking;
using R2API.Networking;
using EntityStates.Badger.Components;
using R2API.Networking.Interfaces;
using FortunesFromTheScrapyard;
using FortunesFromTheScrapyard.Survivors.Badger;

namespace EntityStates.Badger
{
    public class BadgerFireAR : BaseSkillState
    {
        public static float damageCoefficient = BadgerSurvivor.basePrimaryDamage;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.1f; // per shot
        public static float force = 200f;
        public static float recoil = 1.2f; // was 0.5f
        public static float range = 2000f;
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        public static GameObject critTracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCaptainShotgun");
        public static int maxShotsPerBurst = 4;

        private int shotsFired;
        private float duration;
        private string muzzleString;
        private bool isCrit;
        private float timer;

        protected virtual float _damageCoefficient => BadgerFireAR.damageCoefficient;
        protected virtual GameObject tracerPrefab => this.isCrit ? BadgerFireAR.critTracerEffectPrefab : BadgerFireAR.tracerEffectPrefab;
        public virtual string shootSoundString => "Play_commando_R";
        public virtual BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = BadgerFireAR.baseDuration / this.attackSpeedStat;

            this.muzzleString = "GunMuzzle";

            this.isCrit = base.RollCrit();

            base.characterBody.SetAimTimer(2f);

            this.Fire();

            this.PlayCrossfade("Gesture, Override", "FireAR", "Primary.playbackRate", this.duration * 5f, this.duration * 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();

            this.PlayCrossfade("Gesture, Override", "PrimaryIdle", "Primary.playbackRate", this.duration, this.duration * 0.05f);
        }

        private void Fire()
        {
            shotsFired++;

            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, this.gameObject, this.muzzleString, false);

            Util.PlaySound(this.shootSoundString, this.gameObject);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-1f * BadgerFireAR.recoil, -1f * BadgerFireAR.recoil, -0.5f * BadgerFireAR.recoil, 0.5f * BadgerFireAR.recoil);

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = this._damageCoefficient * this.damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = this.falloff,
                    maxDistance = BadgerFireAR.range,
                    force = BadgerFireAR.force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = this.characterBody.spreadBloomAngle * 2.1f, // was 1.5f
                    isCrit = this.isCrit,
                    owner = base.gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = this.tracerPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                };
                bulletAttack.Fire();
            }

            base.characterBody.AddSpreadBloom(1.25f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.fixedDeltaTime;

            if(timer >= this.duration && shotsFired < maxShotsPerBurst)
            {
                timer = 0f;

                base.characterBody.SetAimTimer(2f);

                this.Fire();

                this.PlayCrossfade("Gesture, Override", "FireAR", "Primary.playbackRate", this.duration * 5f, this.duration * 0.05f);
            }

            if (base.fixedAge >= 2f / this.attackSpeedStat && shotsFired == maxShotsPerBurst && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge >= 2f / this.attackSpeedStat) return InterruptPriority.Any;
            return InterruptPriority.PrioritySkill;
        }
    }
}