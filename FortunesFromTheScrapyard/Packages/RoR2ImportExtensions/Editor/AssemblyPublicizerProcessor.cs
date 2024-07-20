using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;
using Debug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

namespace RiskOfThunder.RoR2Importer
{
    public class AssemblyPublicizerProcessor : AssemblyProcessor
    {
        public override int Priority => 400;
        public override string Name => $"Assembly Publicizer Processor";

        public override string Process(string assemblyPath)
        {
            AssemblyPublicizerConfiguration dataStorer = AssemblyPublicizerConfiguration.GetDataStorer();
            //Publicizer not enabled? dont publicize
            if (!dataStorer.enabled)
                return assemblyPath;

            var assemblyFileName = Path.GetFileName(assemblyPath);
            //assembly file name is not in assemblyNames? dont publicize.
            if (!dataStorer.assemblyNames.Contains(assemblyFileName))
                return assemblyPath;

            UObject nstripExe = dataStorer.NStripExecutable;
            if (nstripExe == null)
            {
                Debug.LogWarning($"Could not strip assembly {assemblyFileName}, as NStrip has not been located.");
                return assemblyPath;
            }

            string ror2ManagedDir = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>().ManagedAssembliesPath;

            if (!Directory.Exists(Constants.Paths.PublicizedAssembliesFolder))
            {
                Directory.CreateDirectory(Constants.Paths.PublicizedAssembliesFolder);
            }
            string outputPath = Path.Combine(Constants.Paths.PublicizedAssembliesFolder, assemblyFileName);
            string nstripPath = Path.GetFullPath(AssetDatabase.GetAssetPath(nstripExe));

            List<string> arguments = new List<string>
            {
                "-p",
                "-n",
                "-d", ror2ManagedDir,
                "-cg",
                "--cg-exclude-events",
                "--remove-readonly",
                "--unity-non-serialized",
                assemblyPath,
                outputPath
            };

            List<string> log = new List<string> { $"Publicized {assemblyFileName} with the following arguments:" };
            log.AddRange(StripAssembly(arguments, nstripPath));
            Debug.Log(string.Join("\n", log));

            return outputPath;
        }

        private List<string> StripAssembly(List<string> arguments, string nstripPath)
        {
            var logger = new List<string>();
            for (int i = 0; i < arguments.Count; i++)
            {
                logger.Add($"Argument {i}: {arguments[i]}");
            }

            ProcessStartInfo psi = new ProcessStartInfo(nstripPath)
            {
                WorkingDirectory = Path.GetDirectoryName(nstripPath),
            };

            psi.Arguments = string.Join(" ", arguments.Select(arg => $"\"{arg}\""));

            var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit(5000);
            return logger;
        }
    }
}