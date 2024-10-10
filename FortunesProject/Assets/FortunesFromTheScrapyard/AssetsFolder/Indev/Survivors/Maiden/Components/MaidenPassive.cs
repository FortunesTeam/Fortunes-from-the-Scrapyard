using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Maiden.Components
{
    public class MaidenPassive : MonoBehaviour
    {
        public SkillDef gamblePassive;

        public GenericSkill passiveSkillSlot;

        public bool isGamble => gamblePassive && passiveSkillSlot && passiveSkillSlot.skillDef && passiveSkillSlot == gamblePassive;
    }
}