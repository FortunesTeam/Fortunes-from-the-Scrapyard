using EntityStates;
using FortunesFromTheScrapyard.Survivors;
using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EntityStates.Duke
{
    public class DukeBaseMeleeAttack: BaseSkillState
    {
        public static float baseDuration = 4f;
        private float timer;
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            timer += Time.fixedDeltaTime;

            if (base.isAuthority)
            {
                if (inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down)
                {
                    timer = 0f;
                }

                if (timer >= baseDuration || this.skillLocator.primary.stock == 0)
                {
                    if (base.isAuthority) outer.SetNextState(new Reload());
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
