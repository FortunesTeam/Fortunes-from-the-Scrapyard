namespace RiskOfThunder.RoR2Importer
{
    using System.IO;
    using UnityEngine;
    using TkPriority = ThunderKit.Common.Constants.Priority;
    public static class Constants
    {
        public static class Priority
        {
            public const int PostProcessingInstaller = TkPriority.AssemblyImport + 250_000;
            public const int TextMeshProUninstaller = TkPriority.AssemblyImport + 240_000;
            public const int UGUIUninstaller = TkPriority.AssemblyImport + 230_000;
            public const int AssemblyPublicizerConfiguration = TkPriority.AssemblyImport + 125_000;
            public const int MMHookGeneratorConfiguration = TkPriority.AssemblyImport + 120_000;
            public const int LegacyResourceAPIPatcher = TkPriority.AssemblyImport - 250_000;
            public const int EnsureRoR2Thunderstore = TkPriority.AddressableCatalog - 125_000;
            public const int InstallBepInEx = TkPriority.AddressableCatalog - 135_000;
            public const int InstallR2API = TkPriority.AddressableCatalog - 145_000;
            public const int InstallMLAPI = TkPriority.AddressableCatalog - 155_000;
            public const int InstallRoR2EK = TkPriority.AddressableCatalog - 160_000;
            public const int ThunderstorePackageInstaller = TkPriority.AddressableCatalog - 250_000;
        }

        public static class Paths
        {
            public const string OldMMHookPath = "Packages/riskofthunder-hookgenpatcher/BepInEx/plugins/MMHOOK";
            public const string NStripExePath = "Packages/riskofthunder-ror2importextensions/Binary/NStrip/NStrip.exe";
            public const string HookGenExePath = "Packages/riskofthunder-ror2importextensions/Binary/MonoMod.RuntimeDetour.HookGen/MonoMod.RuntimeDetour.HookGen.exe";
            public static string PublicizedAssembliesFolder
            {
                get
                {
                    string tempFolder = Application.dataPath.Replace("Assets", "Temp");
                    return Path.Combine(tempFolder, "ThunderKit", "PublicizedAssemblies");
                }
            }

            public static string HookGenAssembliesPackageFolder
            {
                get
                {
                    string packagesFolder = Application.dataPath.Replace("Assets", "Packages");
                    return Path.Combine(packagesFolder, "mmhook-assemblies");
                }
            }
        }
    }
}