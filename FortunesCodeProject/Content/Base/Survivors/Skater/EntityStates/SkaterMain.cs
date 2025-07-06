using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using RoR2.Skills;

namespace EntityStates.Skater
{
    public class SkaterMain : GenericCharacterMain
    {
        private GenericSkill skateSkill;
        public override void OnEnter()
        {
            base.OnEnter();
            skateSkill = skillLocator.FindSkillByFamilyName("SkaterPassiveFamily");
            if (skateSkill == null)
            {
                Debug.LogError("skater moment");
            }
        }

        public override void HandleMovements()
        {
            bool skateReady = (bool)skateSkill ? skateSkill.CanExecute() : false;
            if (!skateReady)
            {
                sprintInputReceived = false;
            }

            base.HandleMovements();
            characterBody.isSprinting = false;

            if (isAuthority && sprintInputReceived && skateReady)
            {
                SkillDef skillDef = skateSkill.skillDef;

                skateSkill.hasExecutedSuccessfully = true;
                if (skateSkill.stateMachine.SetInterruptState(skillDef.InstantiateNextState(skateSkill), skillDef.interruptPriority))
                {
                    skateSkill.stock -= skillDef.stockToConsume;
                }
            }
        }
    }
}
