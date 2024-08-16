using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Cloaker.Components
{
    [CreateAssetMenu(fileName = "SkillDefs", menuName = "FortunesFromTheScrapyard/SkillDefs/CloakerMarkSkillDef")]

    public class CloakerMarkSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public CloakerTrackerController tracker;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                tracker = skillSlot.GetComponent<CloakerTrackerController>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if (!(((InstanceData)skillSlot.skillInstanceData).tracker?.GetTrackingTarget()) && !skillSlot.characterBody.hasCloakBuff) 
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


