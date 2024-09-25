using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    [CreateAssetMenu(fileName = "SkillDefs", menuName = "FortunesFromTheScrapyard/SkillDefs/NeuromancerDrainSkillDef")]
    public class NeuromancerDrainSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public CharacterBody body;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                body = skillSlot.gameObject.GetComponent<CharacterBody>(),
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            CharacterBody body = ((NeuromancerDrainSkillDef.InstanceData)skillSlot.skillInstanceData).body;
            bool target = false;
            if (body && !body.gameObject.GetComponent<NeuromancerController>().drainTimeEssence)
            {
                target = true;
            }
            return target;
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