using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerPassive : MonoBehaviour
    {
        public SkillDef passiveSkillDef;

        public SkillDef akimboSkillDef;

        public GenericSkill passiveSkillSlot;

        public bool isCloak
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

        public bool isAkimbo
        {
            get
            {
                if(akimboSkillDef && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == akimboSkillDef;
                }

                return false;
            }
        }
    }
}