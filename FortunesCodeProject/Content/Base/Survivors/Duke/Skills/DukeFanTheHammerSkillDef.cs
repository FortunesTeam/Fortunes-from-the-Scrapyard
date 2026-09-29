using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using EntityStates;

namespace FortunesFromTheScrapyard.Survivors.Duke.Skills
{
    [CreateAssetMenu(menuName = "FortunesFromTheScrapyard/SkillDefs/DukeFanTheHammerSkillDef")]
    public class DukeFanTheHammerSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public SkillLocator skillLocator;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                skillLocator = skillSlot.GetComponent<SkillLocator>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if ((((InstanceData)skillSlot.skillInstanceData).skillLocator.primary.stock == 0))
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
