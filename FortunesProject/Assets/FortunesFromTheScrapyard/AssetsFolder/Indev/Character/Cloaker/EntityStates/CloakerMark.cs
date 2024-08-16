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
        private CloakerTrackerController tracker;

        private HurtBox victim;
        public override void OnEnter()
        {
            base.OnEnter();

            tracker = base.gameObject.GetComponent<CloakerTrackerController>();
            if (tracker)
            {
                if (base.isAuthority) victim = tracker.GetTrackingTarget();

                if (victim && victim.healthComponent.body)
                {
                    if (!victim.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked) && !victim.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarkCd))
                    {
                        if (NetworkServer.active)
                        {
                            victim.healthComponent.body.AddBuff(ScrapyardContent.Buffs.bdCloakerMarked);
                        }
                    }
                }
            }

            if (base.isAuthority) outer.SetNextStateToMain();
        }
    }
}
