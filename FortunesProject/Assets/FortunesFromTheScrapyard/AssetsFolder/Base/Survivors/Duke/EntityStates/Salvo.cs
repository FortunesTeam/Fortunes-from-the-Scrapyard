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
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using RoR2.Networking;
using System.Collections.Generic;
using System.Linq;
using FortunesFromTheScrapyard.Ricochet;

namespace EntityStates.Duke
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

        public static GameObject tracerEffectPrefab = DukeSurvivor.dukeTracer;
        public static GameObject empoweredTracerEffectPrefab = DukeSurvivor.dukeTracerCrit;

        private Vector3 ricochetPoint;
        private bool fourthShot;
        private bool freeBullet;
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
        private DukeController dukeController;
        protected GameObject tracerPrefab = tracerEffectPrefab;
        public virtual string shootSoundString => fourthShot ? "Play_railgunner_R_fire" : "Play_railgunner_m2_alt_fire";
        public BulletAttack.FalloffModel falloff = BulletAttack.FalloffModel.DefaultBullet;

        public override void OnEnter()
        {
            this.dukeController = base.gameObject.GetComponent<DukeController>();
            this.damageCoefficient = DukeSurvivor.baseSalvoDamageCoefficient * (1f + dukeController.attackSpeedConversion);
            this.procCoefficient = baseProcCoefficient;
            this.force = baseForce;
            this.bulletSpread = baseBulletSpread;
            this.bulletRadius = baseBulletRadius;
            this.bulletRecoil = baseBulletRecoil;
            this.bulletRange = baseBulletRange;
            this.selfForce = baseSelfForce;

            base.OnEnter();
            this.dukeController.speedUpReloadTime = false;
            this.windupDuration = baseShootDuration / this.attackSpeedStat;
            this.duration = baseDuration / this.attackSpeedStat;

            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "GunMuzzle";

            if (NetworkServer.active && characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
            {
                freeBullet = true;
                windupDuration /= 4f;
                duration /= 4f;
                characterBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeFreeShot);
                this.dukeController.speedUpReloadTime = true;
            }

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

            PlayAnimations();
        }

        public virtual void PlayAnimations()
        {
            Animator animator = GetModelAnimator();

            if (animator)
            {
                //animator.SetLayerWeight(animator.GetLayerIndex("Gesture, ShootBody"), 1f);

                bool isMoving = animator.GetBool("isMoving");
                bool isGrounded = animator.GetBool("isGrounded");
                if (!isMoving && isGrounded)
                {
                    if (this.isCrit)
                    {
                        this.PlayCrossfade("FullBody, Override", "EnterShootCrit", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.15f);
                    }
                    else
                    {
                        //Play ShootCrit to Shoot Transition when its done
                        this.PlayCrossfade("FullBody, Override", "EnterShoot", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.15f);
                    }                   
                }
                else //If moving
                {
                    if (this.isCrit)
                    {
                        this.PlayCrossfade("Gesture, Additive", "EnterShootCrit", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.25f);
                    }
                    else
                    {
                        //Play ShootCrit to Shoot Transition when its done
                        this.PlayCrossfade("Gesture, Additive", "EnterShoot", "Primary.playbackRate", this.windupDuration, this.windupDuration * 0.25f);
                    }  
                }
            }
        }
        public override void OnExit()
        {
            Animator animator = GetModelAnimator();
            if(animator)
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
            if (this.dukeController)
            {
                //this.dukeController.DropBullet(-this.GetModelBaseTransform().transform.right * -Random.Range(4, 12));
            }

            Util.PlaySound(this.shootSoundString, this.gameObject);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                BulletAttack salvoAttack = new BulletAttack
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
                    trajectoryAimAssistMultiplier = 0.3f
                };

                if (isCrit)
                {
                    salvoAttack.AddModdedDamageType(DukeSurvivor.DukeFourthShot);
                }

                salvoAttack.minSpread = 0;
                salvoAttack.maxSpread = 0;
                salvoAttack.bulletCount = 1;
                salvoAttack.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
                {
                    CharacterBody victimBody = null;
                    if (hitInfo.hurtBox && hitInfo.hitHurtBox)
                    {
                        victimBody = hitInfo.hitHurtBox.hurtBoxGroup.mainHurtBox.healthComponent.body;
                    }
                    if (victimBody && damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (victimBody.gameObject.TryGetComponent<DukeDecoyExplosion>(out var boom))
                        {
                            if (boom.ownerBody == attackerBody)
                            {
                                boom.SetValuesAndKillDecoy(damageInfo.damage / attackerBody.damage, damageInfo.crit);
                            }
                        }
                    }
                };
                
                salvoAttack.Fire();

                this.characterMotor.ApplyForce(aimRay.direction * -this.selfForce);
            }

            base.characterBody.AddSpreadBloom(2.5f);

            hasFired = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot) && !gaveQuickReset)
            {
                this.gaveQuickReset = true;

                this.duration -= this.duration / 1.5f;
            }

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
