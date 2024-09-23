﻿using UnityEngine;
using RoR2;
using EntityStates;
using EntityStates.Commando;
using UnityEngine.Networking;
using R2API;
using FortunesFromTheScrapyard.Survivors.Duke;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using MSU;
using MSU.Config;
using FortunesFromTheScrapyard;

namespace EntityStates.Duke
{
    public class Salvo : BaseSkillState
    {
        public const string TOKEN = "SCRAPYARD_SURVIVOR_DUKE_PRIMARY_DESC";

        [ConfigureField(ScrapyardConfig.ID_SURVIVORS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseDamageCoefficient = 4f;
        public static float baseProcCoefficient = 1f;
        public static float baseDuration = 1.5f;
        public static float baseForce = 600f;
        public static int bulletCount = 1;
        public static float baseBulletSpread = 0f;
        public static float baseBulletRadius = 0.2f;
        public static float baseBulletRecoil = 2f;
        public static float baseBulletRange = 999f;
        public static float baseSelfForce = 750f;

        public static GameObject tracerEffectPrefab = DukeSurvivor.dukeTracer;
        public static GameObject empoweredTracerEffectPrefab = DukeSurvivor.dukeTracerCrit;

        private bool fourthShot;
        private bool freeBullet;
        private float duration;
        private string muzzleString;
        private bool isCrit;
        private float recoil;

        private float damageCoefficient;
        private float procCoefficient;
        private float force;
        private float bulletSpread;
        private float bulletRadius;
        private float bulletRecoil;
        private float bulletRange;
        private float selfForce;
        private DamageType damageType = DamageType.Generic;
        private DukeController dukeController;
        protected GameObject tracerPrefab = tracerEffectPrefab;
        public virtual string shootSoundString => fourthShot ? "Play_railgunner_R_fire" : "Play_railgunner_m2_alt_fire";
        public BulletAttack.FalloffModel falloff = BulletAttack.FalloffModel.DefaultBullet;

        public override void OnEnter()
        {
            this.dukeController = base.gameObject.GetComponent<DukeController>();
            this.damageCoefficient = baseDamageCoefficient;
            this.procCoefficient = baseProcCoefficient;
            this.force = baseForce;
            this.bulletSpread = baseBulletSpread;
            this.bulletRadius = baseBulletRadius;
            this.bulletRecoil = baseBulletRecoil;
            this.bulletRange = baseBulletRange;
            this.selfForce = baseSelfForce;

            base.OnEnter();
            
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "GunMuzzle";

            if (NetworkServer.active && characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
            {
                freeBullet = true;
                characterBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeFreeShot);
            }

            this.duration = baseDuration / this.attackSpeedStat;

            this.isCrit = base.RollCrit();

            this.recoil = bulletRecoil / this.attackSpeedStat;

            if (skillLocator.primary.stock == 0f || freeBullet)
            {
                fourthShot = true;
                isCrit = true;
                force *= 2f;
                bulletRadius *= 2f;
                bulletRecoil *= 2f;
                selfForce *= 2f;
                falloff = BulletAttack.FalloffModel.None;
                damageType |= DamageType.BonusToLowHealth;
            }

            if (freeBullet)
            {
                skillLocator.primary.stock++;
            }

            tracerPrefab = this.isCrit ? empoweredTracerEffectPrefab : tracerEffectPrefab;

            this.Fire();

            this.PlayCrossfade("Gesture, Override", "Shoot", "Shoot.playbackRate", this.duration * 1.5f, this.duration * 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (this.dukeController)
            {
                this.dukeController.DropBullet(-this.GetModelBaseTransform().transform.right * -Random.Range(4, 12));
            }

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
                    stopperMask = fourthShot ? LayerIndex.world.mask : LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = this.tracerPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    HitEffectNormal = false,
                };

                if(fourthShot)
                {
                    bulletAttack.AddModdedDamageType(DukeSurvivor.DukeFourthShot);
                }
                dukeController.ammo--;
                dukeController.onAmmoChange?.Invoke();

                bulletAttack.minSpread = 0;
                bulletAttack.maxSpread = 0;
                bulletAttack.bulletCount = 1;
                bulletAttack.Fire();

                this.characterMotor.ApplyForce(aimRay.direction * -this.selfForce);
            }

            base.characterBody.AddSpreadBloom(2.5f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration || characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
            {
                if(base.isAuthority) this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge >= this.duration) return InterruptPriority.Any;
            return InterruptPriority.PrioritySkill;
        }
    }
}
