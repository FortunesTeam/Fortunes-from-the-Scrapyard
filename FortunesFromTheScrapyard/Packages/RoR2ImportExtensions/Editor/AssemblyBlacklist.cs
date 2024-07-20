using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;

namespace RiskOfThunder.RoR2Importer
{
    public class AssemblyBlacklist : BlacklistProcessor
    {
        public override string Name => "RoR2 Assembly Blacklist";

        public override int Priority => 1_000;

        public override IEnumerable<string> Process(IEnumerable<string> blacklist)
        {
            var importConfiguration = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();

            if (importConfiguration.ConfigurationExecutors.OfType<InstallMultiplayerHLAPI>().Any(ie => ie.enabled))
                blacklist = blacklist.Append("com.unity.multiplayer-hlapi.Runtime.dll");

            if (importConfiguration.ConfigurationExecutors.OfType<PostProcessingInstaller>().Any(ie => ie.enabled))
                blacklist = blacklist.Append("Unity.Postprocessing.Runtime.dll");

            return blacklist;
        }
    }
}