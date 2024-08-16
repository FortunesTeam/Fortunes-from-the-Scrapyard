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
        public virtual BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        private CloakerController cloakerController;
        private float damageStatSnapshot;
        private int step;
        public override void OnEnter()
        {
            this.cloakerController = base.gameObject.GetComponent<CloakerController>();

            base.OnEnter();

            this.damageStatSnapshot = base.characterBody.damage;

            if (this.characterBody.hasCloakBuff)
            {
                if (NetworkServer.active) this.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                this.cloakerController.passiveCloakOn = false;
            }
            this.duration = CloakerShoot.baseDuration / this.attackSpeedStat;
            this.characterBody.isSprinting = false;

            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "MuzzleRight";

            if (this.cloakerController.isAkimbo)
            {
                this.muzzleString = step % 2 == 0 ? "MuzzleRight" : "MuzzleLeft";
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
            this.PlayAnimation("Gesture, Override", "Shoot", "Shoot.playbackRate", this.duration * 1.5f);

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
                    damage = this.baseDamageCoefficient * this.damageStatSnapshot,
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
                if (charged) bulletAttack.AddModdedDamageType(FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker.CloakerChargedDamageType);
                if (muzzleString == "MuzzleLeft") bulletAttack.AddModdedDamageType(FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker.CloakerAkimboDamageType);
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
        }
    }
}