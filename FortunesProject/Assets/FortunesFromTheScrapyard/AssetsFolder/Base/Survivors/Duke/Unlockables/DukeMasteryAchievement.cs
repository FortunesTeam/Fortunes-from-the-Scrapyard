using FortunesFromTheScrapyard.Unlocks;
using RoR2;
using UnityEngine;
namespace FortunesFromTheScrapyard.Unlocks.Duke
{
    public sealed class DukeMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("DukeBody");
        }
    }
}