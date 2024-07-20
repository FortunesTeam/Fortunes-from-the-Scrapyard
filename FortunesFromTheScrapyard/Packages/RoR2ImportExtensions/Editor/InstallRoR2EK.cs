using ThunderKit.Integrations.Thunderstore;

namespace RiskOfThunder.RoR2Importer
{
    public class InstallRoR2EK : ThunderstorePackageInstaller
    {
        public override int Priority => Constants.Priority.InstallRoR2EK;
        public override string ThunderstoreAddress => "https://thunderkit.thunderstore.io";
        public override string DependencyId => "RiskofThunder-RoR2EditorKit";
        public override string Description => $"Installs the RoR2 Editor Kit Package from the ThunderKit Extension store";
        public override string Name => $"Install RoR2 Editor Kit";
    }
}

