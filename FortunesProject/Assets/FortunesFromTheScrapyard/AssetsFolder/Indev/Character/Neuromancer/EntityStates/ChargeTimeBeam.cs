using EntityStates;


using RoR2;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class ChargeTimeBeam : BaseNeuromancerSkillState
    {
        [SerializeField]
        public string animationLayerName = "RightArm, Override";

        [SerializeField]
        public string animationStateName = "ChargeHandBeam";

        [SerializeField]
        public string animationPlaybackRateParam = "HandBeam.playbackRate";

        [SerializeField]
        public float baseDuration = 0.25f;

        [SerializeField]
        public string muzzle = "Muzzle";

        [SerializeField]
        public string enterSoundString = "Play_item_void_bleedOnHit_start";

        [SerializeField]
        public GameObject muzzleflashEffectPrefab = Neuromancer.timeBeamChargeEffect;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            if (neuromancerController.hitMaxOverheat)
            {
                outer.SetNextStateToMain();
                return;
            }

            duration = baseDuration / attackSpeedStat;
            GetAimRay();
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
            base.characterBody.SetAimTimer(3f);
            Util.PlaySound(enterSoundString, base.gameObject);
            if (muzzleflashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzle, transmit: false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (neuromancerController.hitMaxOverheat)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (base.isAuthority && base.fixedAge > duration)
            {
                outer.SetNextState(new Beam
                {
                    activatorSkillSlot = base.activatorSkillSlot
                });
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
