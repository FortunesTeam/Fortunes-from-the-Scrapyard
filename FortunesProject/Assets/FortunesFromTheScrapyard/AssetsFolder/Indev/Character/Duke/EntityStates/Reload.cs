using RoR2;
using UnityEngine;
using EntityStates;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using UnityEngine.AddressableAssets;

namespace EntityStates.Duke
{
    public class Reload : BaseSkillState
    {
        public static float baseDuration = 2.5f;
        private float duration;
        private bool hasGivenStock;
        private bool disabledSound;
        private uint soundID;
        private GameObject spinInstance;
        private DukeController dukeController;

        public override void OnEnter()
        {
            dukeController = base.gameObject.GetComponent<DukeController>();    
            base.OnEnter();
            this.duration = baseDuration / attackSpeedStat;
            //this.dukeController.DropMag(-this.GetModelBaseTransform().transform.right * -Random.Range(4, 12));
            base.PlayCrossfade("Gesture, Override", "Reload", "Reload.playbackRate", this.duration, 0.05f);
            soundID = Util.PlayAttackSpeedSound("sfx_driver_pistol_spin", base.gameObject, attackSpeedStat);
            if (this.spinInstance) GameObject.Destroy(this.spinInstance);
            this.spinInstance = GameObject.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoReloadFX.prefab").WaitForCompletion());
            this.spinInstance.transform.parent = base.GetModelChildLocator().FindChild("Weapon");
            this.spinInstance.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            this.spinInstance.transform.localPosition = Vector3.zero;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.duration / 2f && !disabledSound)
            {
                disabledSound = true;
                AkSoundEngine.StopPlayingID(this.soundID);
                GameObject.Destroy(this.spinInstance);
            }
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                //this.dukeController.Mag();
                Util.PlaySound("sfx_duke_gun_catch", base.gameObject);
                GiveStock();
                dukeController.Reload();
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (!disabledSound)
            {
                AkSoundEngine.StopPlayingID(this.soundID);
                GameObject.Destroy(this.spinInstance);
            }
        }
        private void GiveStock()
        {
            if (!hasGivenStock)
            {
                for (int i = base.skillLocator.primary.stock; i < base.skillLocator.primary.maxStock; i++) base.skillLocator.primary.AddOneStock();
                hasGivenStock = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}