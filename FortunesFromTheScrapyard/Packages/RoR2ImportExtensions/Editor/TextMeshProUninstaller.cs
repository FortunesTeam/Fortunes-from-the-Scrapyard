using ThunderKit.Core.Config.Common;

namespace RiskOfThunder.RoR2Importer
{
    public class TextMeshProUninstaller : UnityPackageUninstaller
    {
        public override string Name => "TextMeshPro Uninstaller";
        public override string Description => $"Removes Unity TextMeshPro due to compatibility issues with the games modified TextMeshPro library and ensures that Unity.TextMeshPro.dll is copied from the games directory";
        public override int Priority => Constants.Priority.TextMeshProUninstaller;
        public override string PackageIdentifier => "com.unity.textmeshpro";
    }
}