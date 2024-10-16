using UnityEngine;
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
    public class FanTheHammer : BaseSkillState
    {
        public static float baseProcCoefficient = 1f;
        public static float baseWindupDuration = 0.5f;
        public static float baseDurationPerShot = 0.25f;
        public static float baseForce = 600f;
        public static int bulletCount = 1;
        public static float baseBulletSpread = 10f;
        public static float baseBulletRadius = 0.2f;
        public static float baseBulletRecoil = 4f;
        public static float baseBulletRange = 999f;
        public static float baseSelfForce = 200f;

        public static GameObject tracerEffectPrefab = DukeSurvivor.dukeTracer;
        public static GameObject empoweredTracerEffectPrefab = DukeSurvivor.dukeTracerCrit;
        private uint soundID;
        private GameObject spinInstance;

        private bool fourthShot;
        private bool freeBullet;
        private float duration;
        private float windupDuration;
        private float perShotDuration;
        private string muzzleString;
        private bool isCrit;

        private float damageCoefficient;
        private float procCoefficient;
        private float force;
        private float bulletSpread;
        private float bulletRadius;
        private float bulletRecoil;
        private float bulletRange;
        private float selfForce;
        private bool hasFiredAllShots;
        private DamageType damageType = DamageType.Generic;
        private DukeController dukeController;
        private int maxShotCount;
        private float shotTimer;
        private bool disabledSound;

        protected GameObject tracerPrefab = tracerEffectPrefab;
        public virtual string shootSoundString => fourthShot ? "Play_railgunner_R_fire" : "Play_railgunner_m2_alt_fire";
        public BulletAttack.FalloffModel falloff = BulletAttack.FalloffModel.DefaultBullet;

        public override void OnEnter()
        {
            this.skillLocator.primary.stock = this.skillLocator.primary.maxStock;
            this.dukeController = base.gameObject.GetComponent<DukeController>();

            this.damageCoefficient = DukeSurvivor.baseFanDamageCoefficient * (1f + dukeController.attackSpeedConversion);
            this.procCoefficient = baseProcCoefficient;
            this.force = baseForce;
            this.bulletSpread = baseBulletSpread;
            this.bulletRadius = baseBulletRadius;
            this.bulletRecoil = (baseBulletRecoil / 4f) / this.attackSpeedStat;
            this.bulletRange = baseBulletRange;
            this.selfForce = baseSelfForce;
            this.freeBullet = false;
            this.fourthShot = false;
            this.isCrit = RollCrit();
            this.falloff = BulletAttack.FalloffModel.DefaultBullet;
            this.damageType = DamageType.Generic;

            base.OnEnter();

            this.maxShotCount = this.skillLocator.primary.GetBaseMaxStock() + characterBody.GetBuffCount(ScrapyardContent.Buffs.bdDukeFreeShot);

            soundID = Util.PlayAttackSpeedSound("sfx_duke_pistol_spin", base.gameObject, attackSpeedStat);
            if (this.spinInstance) GameObject.Destroy(this.spinInstance);
            this.spinInstance = GameObject.Instantiate(DukeSurvivor.dukePistolSpinEffect);
            this.spinInstance.transform.parent = base.GetModelChildLocator().FindChild("Weapon");
            this.spinInstance.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
            this.spinInstance.transform.localPosition = Vector3.zero;

            this.windupDuration = baseWindupDuration / this.attackSpeedStat;
            this.perShotDuration = baseDurationPerShot / this.attackSpeedStat;
            this.duration = maxShotCount * perShotDuration + windupDuration;

            Animator animator = GetModelAnimator();

            //animator.SetLayerWeight(animator.GetLayerIndex("Gesture, ShootBody"), 1f);

            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                base.PlayCrossfade("FullBody, Override", "DeployFan", "Special.playbackRate", this.windupDuration, 0.05f);
            }
            else base.PlayCrossfade("Gesture, Additive", "DeployFan", "Special.playbackRate", this.windupDuration, 0.05f);
        }

        public override void OnExit()
        {
            if (!disabledSound)
            {
                AkSoundEngine.StopPlayingID(this.soundID);
                GameObject.Destroy(this.spinInstance);
            }
            Animator animator = GetModelAnimator();
            if (animator)
            {
                //animator.SetLayerWeight(animator.GetLayerIndex("Gesture, ShootBody"), 0f);
            }
            base.OnExit();
        }

        private void Fire()
        {
            if (this.dukeController)
            {
                //this.dukeController.DropBullet(-this.GetModelBaseTransform().transform.right * -Random.Range(4, 12));
            }
            Animator animator = GetModelAnimator();
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                base.PlayCrossfade("FullBody, Override", "ShootFan", "Special.playbackRate", this.windupDuration, 0.05f);
            }
            else base.PlayCrossfade("Gesture, Additive", "ShootFan", "Special.playbackRate", this.windupDuration, 0.05f);

            Util.PlaySound(this.shootSoundString, this.gameObject);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-0.4f * bulletRecoil, -0.8f * bulletRecoil, -0.3f * bulletRecoil, 0.3f * bulletRecoil);

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
                    hitMask = LayerIndex.world.mask | LayerIndex.entityPrecise.mask | LayerIndex.noCollision.mask,
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
                    stopperMask = (fourthShot ? LayerIndex.world.mask : default(LayerMask)),
                    weapon = null,
                    tracerEffectPrefab = this.tracerPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    HitEffectNormal = false,
                };

                if (isCrit)
                {
                    bulletAttack.AddModdedDamageType(DukeSurvivor.DukeFourthShot);
                }

                bulletAttack.minSpread = 0;
                bulletAttack.maxSpread = 0;
                bulletAttack.bulletCount = 1;
                bulletAttack.Fire();

                this.characterMotor.ApplyForce(aimRay.direction * -this.selfForce);

                ResetShot();
            }

            base.characterBody.AddSpreadBloom(2.5f);
        }

        public void ResetShot()
        {
            this.damageCoefficient = DukeSurvivor.baseSalvoDamageCoefficient;
            this.procCoefficient = baseProcCoefficient;
            this.force = baseForce;
            this.bulletSpread = baseBulletSpread;
            this.bulletRadius = baseBulletRadius;
            this.bulletRecoil = (baseBulletRecoil / 4f) / this.attackSpeedStat;
            this.bulletRange = baseBulletRange;
            this.selfForce = baseSelfForce;
            this.freeBullet = false;
            this.fourthShot = false;
            this.isCrit = RollCrit();
            this.falloff = BulletAttack.FalloffModel.DefaultBullet;
            this.damageType = DamageType.Generic;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.windupDuration && skillLocator.primary.stock > 0)
            {
                if (!disabledSound)
                {
                    AkSoundEngine.StopPlayingID(this.soundID);
                    GameObject.Destroy(this.spinInstance);
                }

                shotTimer += Time.fixedDeltaTime;

                if(shotTimer >= perShotDuration)
                {
                    shotTimer = 0;
                    base.characterBody.SetAimTimer(2f);
                    this.muzzleString = "GunMuzzle";

                    if (NetworkServer.active && characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
                    {
                        freeBullet = true;
                        characterBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeFreeShot);
                    }

                    if (skillLocator.primary.stock == 1f || freeBullet)
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

                    tracerPrefab = this.isCrit ? empoweredTracerEffectPrefab : tracerEffectPrefab;

                    this.Fire();

                    skillLocator.primary.stock--;
                }
            }

            if (base.isAuthority && skillLocator.primary.stock <= 0)
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
