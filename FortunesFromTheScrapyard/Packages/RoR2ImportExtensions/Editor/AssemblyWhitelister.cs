using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;

namespace RiskOfThunder.RoR2Importer
{
    public class AssemblyWhitelister : WhitelistProcessor
    {
        public override string Name => "RoR2 Assembly Whitelist";

        public override int Priority => 750;

        public override IEnumerable<string> Process(IEnumerable<string> whitelist)
        {
            var importConfiguration = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();

            if (importConfiguration.ConfigurationExecutors.OfType<TextMeshProUninstaller>().Any(ie => ie.enabled))
                whitelist = whitelist.Append("Unity.TextMeshPro.dll");

            if (importConfiguration.ConfigurationExecutors.OfType<UGUIUninstaller>().Any(ie => ie.enabled))
                whitelist = whitelist.Append("UnityEngine.UI.dll");
            return whitelist;
        }
    }
}