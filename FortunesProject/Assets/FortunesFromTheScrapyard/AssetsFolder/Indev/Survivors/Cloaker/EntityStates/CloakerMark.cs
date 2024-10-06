using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using RoR2;
using FortunesFromTheScrapyard;
using UnityEngine.Networking;

namespace EntityStates.Cloaker
{
    public class CloakerMark : BaseSkillState
    {   
        public static float baseDuration = 0.5f;

        private CloakerTrackerController tracker;

        private HurtBox victim;

        private CharacterBody victimBody;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / this.attackSpeedStat; 
            tracker = base.gameObject.GetComponent<CloakerTrackerController>();

            if (base.isAuthority && tracker)
            {
                victim = this.tracker.GetTrackingTarget();
            }

            if (victim && victim.healthComponent.body) victimBody = victim.healthComponent.body;

            if (!victim || !victimBody || !tracker)
            {
                this.skillLocator.special.AddOneStock();
                this.outer.SetNextStateToMain();
                return;
            }

            StartAimMode(this.duration);

            this.PlayCrossfade("Gesture, Additive", Animator.StringToHash("Special2"), this.duration * 0.05f);

            if(NetworkServer.active)
            {
                victimBody.AddBuff(ScrapyardContent.Buffs.bdCloakerMarked);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge >= this.duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            victim = null;
            victimBody = null;
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(HurtBoxReference.FromHurtBox(victim));
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            victim = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
    }
}
