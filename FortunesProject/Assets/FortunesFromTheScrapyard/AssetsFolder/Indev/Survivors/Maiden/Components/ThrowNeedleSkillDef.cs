using System;
using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using FortunesFromTheScrapyard.Survivors.Maiden.Components;

namespace FortunesFromTheScrapyard.Survivors.Maiden.Skills
{
    public class ThrowNeedleSkillDef : SkillDef
    {
        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new ThrowNeedleSkillDef.InstanceData
            {
                tracker = skillSlot.GetComponent<ThrowNeedleTracker>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            ThrowNeedleTracker tracker = ((ThrowNeedleSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
            return (tracker != null) ? tracker.GetTrackingTarget() : null;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return ThrowNeedleSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && ThrowNeedleSkillDef.HasTarget(skillSlot);
        }

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public ThrowNeedleTracker tracker;
        }
    }
}
