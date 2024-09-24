using EntityStates;
using FortunesFromTheScrapyard.Survivors;
using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Duke
{
    public class EnterReload : BaseSkillState
    {
        public static float baseDuration = 0.1f;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= baseDuration && this.skillLocator.primary.stock == 0)
            {
                if (base.isAuthority) outer.SetNextState(new Reload());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
