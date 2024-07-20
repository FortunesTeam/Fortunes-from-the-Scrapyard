using ThunderKit.Integrations.Thunderstore;

namespace RiskOfThunder.RoR2Importer
{
    public class InstallMultiplayerHLAPI : ThunderstorePackageInstaller
    {
        public override int Priority => Constants.Priority.InstallMLAPI;
        public override string ThunderstoreAddress => "https://thunderkit.thunderstore.io";
        public override string DependencyId => "RiskofThunder-RoR2MultiplayerHLAPI";
        public override string Description => $"Installs the RoR2MultiplayerHLAPI Package from the ThunderKit Extension store";
        public override string Name => $"Install RoR2MultiplayerHLAPI";
    }
}

