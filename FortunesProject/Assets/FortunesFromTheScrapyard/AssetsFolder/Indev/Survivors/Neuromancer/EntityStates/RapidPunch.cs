using EntityStates;


using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Neuromancer;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace EntityStates.Neuromancer
{
    public class RapidPunch : BaseNeuromancerSkillState
    {
        [SerializeField]
        public GameObject muzzleflashEffectPrefab = NeuromancerSurvivor.neuroMuzzleFlash;

        [SerializeField]
        public GameObject hitEffectPrefab = null;

        [SerializeField]
        public GameObject beamVfxPrefab = NeuromancerSurvivor.timeBeamEffect;

        [SerializeField]
        public string enterSoundString = "Play_voidman_m1_corrupted_start";

        [SerializeField]
        public string exitSoundString = "Play_voidman_m1_corrupted_end";

        [SerializeField]
        public float tickRate = 8f;

        [SerializeField]
        public float damageCoefficientPerSecond = 10f;

        [SerializeField]
        public float procCoefficientPerSecond = 5f;

        [SerializeField]
        public float forcePerSecond = 200f;

        [SerializeField]
        public float maxDistance = 5f;

        [SerializeField]
        public float bulletRadius = 1f;

        [SerializeField]
        public float baseMinimumDuration = 0.3f;

        [SerializeField]
        public float recoilAmplitude = 0.2f;

        [SerializeField]
        public float spreadBloomValue = 0.1f;

        [SerializeField]
        public float maxSpread = 1f;

        [SerializeField]
        public string muzzle = "Muzzle";

        [SerializeField]
        public string animationLayerName = "RightArm, Override";

        [SerializeField]
        public string animationEnterStateName = "FireHandBeam";

        [SerializeField]
        public string animationExitStateName = "ExitHandBeam";

        private GameObject blinkVfxInstance;

        private float minimumDuration;

        private float fireCountdown;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            minimumDuration = baseMinimumDuration / attackSpeedStat;
            PlayAnimation(animationLayerName, animationEnterStateName);
            Util.PlaySound(enterSoundString, base.gameObject);
            blinkVfxInstance = Object.Instantiate(beamVfxPrefab);
            blinkVfxInstance.transform.SetParent(base.characterBody.aimOriginTransform, worldPositionStays: false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            fireCountdown -= Time.fixedDeltaTime;
            
            if (fireCountdown <= 0f)
            {
                fireCountdown = 1f / tickRate / attackSpeedStat;
                FireBullet();
            }
            base.characterBody.SetAimTimer(3f);
            if (blinkVfxInstance)
            {
                Vector3 point = GetAimRay().GetPoint(maxDistance);
                if (Util.CharacterRaycast(base.gameObject, GetAimRay(), out var hitInfo, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                {
                    point = hitInfo.point;
                }
                blinkVfxInstance.transform.forward = point - blinkVfxInstance.transform.position;
            }
            if (base.fixedAge >= minimumDuration && !IsKeyDownAuthority() && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
            
        }

        public override void OnExit()
        {
            if (blinkVfxInstance)
            {
                VfxKillBehavior.KillVfxObject(blinkVfxInstance);
            }
            PlayAnimation(animationLayerName, animationExitStateName);
            Util.PlaySound(exitSoundString, base.gameObject);
            base.OnExit();
        }

        private void FireBullet()
        {
            Ray aimRay = GetAimRay();
            AddRecoil(-1f * recoilAmplitude, -2f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
            if (muzzleflashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: true);
            }
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.muzzleName = muzzle;
                bulletAttack.maxDistance = maxDistance;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = maxSpread;
                bulletAttack.radius = bulletRadius;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.smartCollision = false;
                bulletAttack.stopperMask = default(LayerMask);
                bulletAttack.hitMask = LayerIndex.entityPrecise.mask;
                bulletAttack.damage = damageCoefficientPerSecond * damageStat / tickRate;
                bulletAttack.procCoefficient = procCoefficientPerSecond / tickRate;
                bulletAttack.force = forcePerSecond / tickRate;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.AddModdedDamageType(NeuromancerSurvivor.DelayedPunch);
                bulletAttack.Fire();
            }
            base.characterBody.AddSpreadBloom(spreadBloomValue);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
