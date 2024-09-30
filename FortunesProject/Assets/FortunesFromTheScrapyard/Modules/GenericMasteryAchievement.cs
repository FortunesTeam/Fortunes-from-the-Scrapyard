using RoR2;
using RoR2.Achievements;
namespace FortunesFromTheScrapyard.Unlocks
{
    public abstract class GenericMasteryAchievement : BaseAchievement
    {
        public virtual float RequiredDifficultyCoefficient { get; }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Run.onClientGameOverGlobal += OnClientGameOverGlobal;
        }
        public override void OnBodyRequirementBroken()
        {
            Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
            base.OnBodyRequirementBroken();
        }
        private void OnClientGameOverGlobal(Run run, RunReport runReport)
        {
            if ((bool)runReport.gameEnding && runReport.gameEnding.isWin)
            {
                DifficultyIndex difficultyIndex = runReport.ruleBook.FindDifficulty();
                DifficultyDef runDifficulty = DifficultyCatalog.GetDifficultyDef(difficultyIndex);

                if ((runDifficulty.countsAsHardMode && runDifficulty.scalingValue >= RequiredDifficultyCoefficient) || //check our required difficulty coefficient
                    (difficultyIndex >= DifficultyIndex.Eclipse1 && difficultyIndex <= DifficultyIndex.Eclipse8) || //check for eclipse (to be consistent with other grand masteries)
                    (runDifficulty.nameToken == "INFERNO_NAME")) //check for inferno mod, as it starts at monsoon coeff but scales higher
                {
                    Grant();
                }
            }
        }
    }
}