using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ThunderKit.Core.Config;
using UnityEditor;
using Debug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

namespace RiskOfThunder.RoR2Importer
{
    public class MMHookGeneratorProcessor : AssemblyProcessor
    {
        private static List<string> CachedAssembliesInAppDomain => cachedAssembliesInAppDomain ?? (cachedAssembliesInAppDomain = GetAssemblesInAppDomain());
        private static List<string> cachedAssembliesInAppDomain;

        public override int Priority => 500;
        public override string Name => $"MMHook Generator Processor";

        public override string Process(string assemblyPath)
        {
            MMHookGeneratorConfiguration dataStorer = MMHookGeneratorConfiguration.GetDataStorer();
            //Publicizer not enabled? dont publicize
            if (!dataStorer.enabled)
                return assemblyPath;

            var assemblyFileName = Path.GetFileName(assemblyPath);
            //assembly file name is not in assemblyNames? dont publicize.
            if (!dataStorer.assemblyNames.Contains(assemblyFileName))
                return assemblyPath;

            UObject hookGenExe = dataStorer.hookGenExecutable;
            if (hookGenExe == null)
            {
                Debug.LogWarning($"Could not generate hook assembly for {assemblyFileName}, as HookGen has not been located.");
                return assemblyPath;
            }

            string outputPath = Path.Combine(Constants.Paths.HookGenAssembliesPackageFolder, $"MMHOOK_{assemblyFileName}");

            if (CachedAssembliesInAppDomain.Contains($"MMHOOK_{assemblyFileName}.dll"))
            {
                Debug.Log($"Not generating hook assembly for {assemblyFileName}, as there's already an assembly with the same name in the app domain.");
                return assemblyPath;
            }

            string hookGenPath = Path.GetFullPath(AssetDatabase.GetAssetPath(hookGenExe));

            List<string> arguments = new List<string>
            {
                "--private",
                assemblyPath,
                outputPath
            };

            List<string> log = new List<string> { $"Generating hook assembly for {assemblyFileName} with the following arguments:" };
            log.AddRange(GenerateHooks(arguments, hookGenPath));
            Debug.Log(string.Join("\n", log));

            return assemblyPath;
        }

        private List<string> GenerateHooks(List<string> argumentList, string hookGenPath)
        {
            var logger = new List<string>();
            for (int i = 0; i < argumentList.Count; i++)
            {
                logger.Add($"Argument {i}: {argumentList[i]}");
            }

            ProcessStartInfo psi = new ProcessStartInfo(hookGenPath)
            {
                WorkingDirectory = Path.GetDirectoryName(hookGenPath),
                UseShellExecute = true,
            };

            string args = string.Join(" ", argumentList.Select(arg => $"\"{arg}\""));
            psi.Arguments = args;

            var process = System.Diagnostics.Process.Start(psi);
            process.WaitForExit(5000);
            return logger;
        }

        private static List<string> GetAssemblesInAppDomain()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass != null)
                .Where(ass => !ass.IsDynamic)
                .Select(ass => ass.Location)
                .Where(ass => !(string.IsNullOrEmpty(ass)))
                .Select(path => Path.GetFileName(path))
                .ToList();
        }
    }
}