using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Config;

namespace Fortunes.Editor
{
    public sealed class BurstAssemblyBlacklist : BlacklistProcessor
    {
        private static readonly string[] AssemblyNames =
        {
            "Unity.Burst.dll",
            "Unity.Burst.Unsafe.dll",
            "Unity.Collections.dll",
            "Unity.Collections.LowLevel.ILSupport.dll",
            "Unity.Mathematics.dll"
        };

        public override string Name => "Burst Assembly Blacklist";
        public override int Priority => 9_001;

        public override IEnumerable<string> Process(IEnumerable<string> blacklist)
        {
            return blacklist.Concat(AssemblyNames);
        }

        internal static void RemoveImportedAssemblies()
        {
            string packagePath = Path.Combine("Packages", "Risk of Rain 2");
            if (!Directory.Exists(packagePath))
                return;

            // ThunderKit whitelists existing game DLLs, overriding the assembly blacklist.
            foreach (string assemblyName in AssemblyNames)
            {
                string path = Path.Combine(packagePath, assemblyName);
                File.Delete(path);
                File.Delete(path + ".meta");
            }
        }
    }
}
