using RoR2;
using UnityEngine;
using EntityStates;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using UnityEngine.AddressableAssets;
using FortunesFromTheScrapyard.Survivors.Duke;

namespace EntityStates.Duke
{
    public class Reload : BaseSkillState
    {
        public static float baseDuration = 2f;
        private float duration;
        private bool hasGivenStock;
        private bool disabledSound;
        private uint soundID;
        private DukeController dukeController;

        public override void OnEnter()
        {
            this.dukeController = base.gameObject.GetComponent<DukeController>();    
            base.OnEnter();
            
            this.duration = this.dukeController.speedUpReloadTime ? ((baseDuration / 2f) / attackSpeedStat) : baseDuration / attackSpeedStat;
            //this.dukeController.DropMag(-this.GetModelBaseTransform().transform.right * -Random.Range(4, 12));
            base.PlayCrossfade("Gesture, Additive", "Reload", "Reload.playbackRate", this.duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.duration / 2f && !disabledSound)
            {
                disabledSound = true;
                AkSoundEngine.StopPlayingID(this.soundID);
            }
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                //this.dukeController.Mag();
                Util.PlaySound("sfx_duke_gun_catch", base.gameObject);
                GiveStock();
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!disabledSound)
            {
                AkSoundEngine.StopPlayingID(this.soundID);
            }
        }
        private void GiveStock()
        {
            if (!hasGivenStock)
            {
                base.skillLocator.primary.stock = base.skillLocator.primary.GetBaseMaxStock();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}