using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.Commando;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Cloaker.Components;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using RoR2.Skills;

namespace EntityStates.Cloaker.Weapon
{
    public class CloakerShoot : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static int ShootStateHash = Animator.StringToHash("Shoot");
        public static int ShootSecondaryStateHash = Animator.StringToHash("ShootSecondary");
        public float baseDamageCoefficient = 2.6f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.4f;
        public static float force = 200f;
        public static float recoil = 2f;
        public static float range = 2000f;
        public GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");
        public GameObject critTracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCaptainShotgun");
        public GameObject hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;
        public bool charged = false;
        protected float duration;
        protected string muzzleString;
        protected bool isCrit;
        protected virtual GameObject tracerPrefab => this.isCrit ? critTracerEffectPrefab : tracerEffectPrefab;
        public string shootSoundString = "";
        public string animationString;
        public virtual BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        private CloakerController cloakerController;
        private int step;

        public override void OnEnter()
        {
            if (!cloakerController)
            {
                this.cloakerController = base.gameObject.GetComponent<CloakerController>();
            }
            

            base.OnEnter();
            characterBody.SetAimTimer(2f);
            this.duration = CloakerShoot.baseDuration / this.attackSpeedStat;
            switch (step)
            {
                default:
                case 0:
                    animationString = "Shoot";
                    muzzleString = "MuzzleRight";
                    break;
                case 1:
                    duration *= 0.4f;
                    animationString = "ShootDual1";
                    muzzleString = "MuzzleRight";
                    break;
                case 2:
                    duration *= 0.4f;
                    animationString = "ShootDual2";
                    muzzleString = "MuzzleLeft";
                    break;
            }

            this.isCrit = base.RollCrit();

            this.shootSoundString = this.isCrit ? "sfx_spy_revolver_shoot_crit" : "sfx_spy_revolver_shoot";
            if (base.isAuthority)
            {
                this.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public void Fire()
        {
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, this.gameObject, this.muzzleString, false);

            Util.PlaySound(this.shootSoundString, this.gameObject);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-0.5f * CloakerShoot.recoil, -0.5f * CloakerShoot.recoil, -0.5f * CloakerShoot.recoil, 0.5f * CloakerShoot.recoil);

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = this.baseDamageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = this.falloff,
                    maxDistance = CloakerShoot.range,
                    force = CloakerShoot.force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = this.characterBody.spreadBloomAngle * 2f,
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
                    hitEffectPrefab = hitEffectPrefab,
                };

                PlayAnimation();

                if (charged)
                {
                    bulletAttack.AddModdedDamageType(FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker.CloakerChargedDamageType);
                }

                if (step == 2) bulletAttack.AddModdedDamageType(FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker.CloakerAkimboDamageType);
                bulletAttack.Fire();
            }

            base.characterBody.AddSpreadBloom(1.25f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void SetStep(int i)
        {
            step = i;
            if (!cloakerController)
            {
                this.cloakerController = activatorSkillSlot.gameObject.GetComponent<CloakerController>();
            }
            if (cloakerController)
            {
                step = cloakerController.isAkimbo ? i + 1 : 0;
            }
        }

        public void PlayAnimation()
        {
            switch (step)
            {
                case int _ when charged:
                    this.PlayCrossfade("Gesture, Additive", ShootSecondaryStateHash, this.duration * 0.05f);
                    break;
                case 0:
                    this.PlayCrossfade("Gesture, Override", animationString, this.duration * 0.1f);
                    break;
                case 1:
                case 2:
                    this.PlayAnimation("Gesture, Override", animationString);
                    break;
            }
        }
    }
}