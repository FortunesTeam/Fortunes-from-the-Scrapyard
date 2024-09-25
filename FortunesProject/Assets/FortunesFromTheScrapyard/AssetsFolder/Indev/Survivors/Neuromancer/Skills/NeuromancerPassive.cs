using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class NeuromancerPassive : MonoBehaviour
    {
        public SkillDef passiveSkillDef;

        public GenericSkill passiveSkillSlot;

        public bool isChromatic
        {
            get
            {
                if (passiveSkillDef && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == passiveSkillDef;
                }

                return false;
            }
        }
    }
}