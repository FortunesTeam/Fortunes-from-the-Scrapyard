using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    public class DukePassive : MonoBehaviour
    {
        public SkillDef fourthShotPassive;

        public GenericSkill passiveSkillSlot;
        public bool isFourthShot
        {
            get
            {
                if (fourthShotPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == fourthShotPassive;
                }

                return false;
            }
        }
    }
}