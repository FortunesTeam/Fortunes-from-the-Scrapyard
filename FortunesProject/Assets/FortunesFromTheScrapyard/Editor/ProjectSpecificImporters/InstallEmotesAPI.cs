using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderKit.Core;
using ThunderKit.Core.Config;
using ThunderKit.Integrations.Thunderstore;

namespace FortunesFromTheScrapyard.Editor
{
    public class InstallEmotesAPI : ThunderstorePackageInstaller
    {
        public override string DependencyId => "MetrosexualFruitcake-CustomEmotesAPI";

        public override string ThunderstoreAddress => "https://thunderstore.io";

        public override int Priority => RiskOfThunder.RoR2Importer.Constants.Priority.InstallMHLAPI - 1000;
    }
}