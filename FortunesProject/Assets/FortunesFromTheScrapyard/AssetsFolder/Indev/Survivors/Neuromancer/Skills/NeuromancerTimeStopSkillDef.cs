using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    [CreateAssetMenu(fileName = "SkillDefs", menuName = "FortunesFromTheScrapyard/SkillDefs/NeuromancerTimeStopSkillDef")]
    public class NeuromancerTimeStopSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public NeuromancerController controller;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                controller = skillSlot.GetComponent<NeuromancerController>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if ((((InstanceData)skillSlot.skillInstanceData).controller.currentTimeEssence < ((InstanceData)skillSlot.skillInstanceData).controller.maxTimeEssence / 2f )
                && ((InstanceData)skillSlot.skillInstanceData).controller.drainTimeEssence == false)
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


