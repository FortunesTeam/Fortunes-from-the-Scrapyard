using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    [CreateAssetMenu(fileName = "SkillDefs", menuName = "FortunesFromTheScrapyard/SkillDefs/NeuromancerTrackerSkillDef")]

    public class NeuromancerTrackerSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public NeuromancerTracker tracker;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                tracker = skillSlot.GetComponent<NeuromancerTracker>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if (!(((InstanceData)skillSlot.skillInstanceData).tracker?.GetTrackingTarget()))
            {
                return false;
            }
            return true;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            if (!HasTarget(skillSlot))
            {
                return false;
            }
            return base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return HasTarget(skillSlot);
            }
            return false;
        }
    }
}


