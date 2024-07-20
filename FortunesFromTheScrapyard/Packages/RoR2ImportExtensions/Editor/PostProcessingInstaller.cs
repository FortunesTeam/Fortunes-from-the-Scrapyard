using ThunderKit.Core.Config.Common;

namespace RiskOfThunder.RoR2Importer
{
    public class PostProcessingInstaller : UnityPackageInstaller
    {
        public override string Name => "PostProcessing Unity Package Installer";
        public override string PackageIdentifier => "com.unity.postprocessing@2.3.0";
        public override string Description => $"Installs Unity PostProcessing Version 2.3.0 and prevents the games Unity.PostProcessing.Runtime.dll from being installed";
        public override int Priority => Constants.Priority.PostProcessingInstaller;
    }
}