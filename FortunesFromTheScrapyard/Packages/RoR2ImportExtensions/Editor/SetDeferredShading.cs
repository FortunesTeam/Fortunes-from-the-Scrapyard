using ThunderKit.Core.Config;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace RiskOfThunder.RoR2Importer
{
    public class SetDeferredShading : OptionalExecutor
    {
        public override int Priority => ThunderKit.Common.Constants.Priority.ProjectSettingsImport - 500;
        public override string Name => "Set Deferred Shading";
        public override string Description => "Sets the Rendering Path for all Graphics Tiers to Deferred";

        public override bool Execute()
        {
            UpdateTier(GraphicsTier.Tier1);
            UpdateTier(GraphicsTier.Tier2);
            UpdateTier(GraphicsTier.Tier3);
            return true;
        }

        private static void UpdateTier(GraphicsTier tier)
        {
            var tierSettings = EditorGraphicsSettings.GetTierSettings(UnityEditor.BuildTargetGroup.Standalone, tier);
            tierSettings.renderingPath = UnityEngine.RenderingPath.DeferredShading;
            tierSettings.enableLPPV = false;
            EditorGraphicsSettings.SetTierSettings(UnityEditor.BuildTargetGroup.Standalone, tier, tierSettings);
        }
    }
}