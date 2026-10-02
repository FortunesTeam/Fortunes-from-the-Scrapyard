using System;
using System.IO;
using System.Linq;
using RiskOfThunder.RoR2Importer;
using ThunderKit.Common.Configuration;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

[assembly: ImportExtensions]

namespace Fortunes.Editor
{
    public sealed class PrepareImportTools : OptionalExecutor
    {
        private const string PackagePath = "Packages/riskofthunder-ror2importextensions";
        private const string ToolsPath = "Assets/ThunderKitTools/Editor";

        public override string Name => "Prepare RoR2 Import Tools";
        public override string Description => "Copies the pinned import tools out of the UPM cache so HookGen and NStrip can execute.";
        public override int Priority => ThunderKit.Common.Constants.Priority.AssemblyImport + 150_000;

        [InitializeOnLoadMethod]
        private static void InitializeImportDefines()
        {
            EditorApplication.quitting -= ClearAddressableDefine;
            EditorApplication.quitting += ClearAddressableDefine;
            if (!File.Exists("Packages/Risk of Rain 2/package.json") ||
                !File.Exists("Packages/Risk of Rain 2/RoR2.dll"))
            {
                ScriptingSymbolManager.RemoveScriptingDefine("RISKOFRAIN2");
                ScriptingSymbolManager.RemoveScriptingDefine("TK_ADDRESSABLE");
            }
            else
            {
                EditorApplication.update -= RestoreAddressableDefine;
                EditorApplication.update += RestoreAddressableDefine;
            }
        }

        private static void ClearAddressableDefine()
        {
            // The next session must import settings before ThunderKit initializes Addressables.
            ScriptingSymbolManager.RemoveScriptingDefine("TK_ADDRESSABLE");
        }

        private static void RestoreAddressableDefine()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            EditorApplication.update -= RestoreAddressableDefine;
            if (File.Exists("Assets/StreamingAssets/aa/settings.json") &&
                File.Exists("Assets/StreamingAssets/aa/catalog.json"))
                ScriptingSymbolManager.AddScriptingDefine("TK_ADDRESSABLE");
        }

        public override bool Execute()
        {
            BurstAssemblyBlacklist.RemoveImportedAssemblies();
            var configuration = ThunderKitSetting.GetOrCreateSettings<ImportConfiguration>();
            var publicizer = configuration.ConfigurationExecutors.OfType<AssemblyPublicizerConfiguration>().Single();
            var hookGenerator = configuration.ConfigurationExecutors.OfType<MMHookGeneratorConfiguration>().Single();
            var package = PackageInfo.FindForAssetPath(PackagePath + "/package.json");
            if (package == null)
                throw new InvalidOperationException("RoR2ImportExtensions must be installed before preparing its import tools.");

            if (publicizer.enabled)
            {
                publicizer.NStripExecutable = CopyTool(package.resolvedPath, "NStrip", "NStrip.exe", "LICENSE.md");
                EditorUtility.SetDirty(publicizer);
            }

            if (hookGenerator.enabled)
            {
                hookGenerator.hookGenExecutable = CopyTool(package.resolvedPath, "MonoMod.RuntimeDetour.HookGen",
                    "MonoMod.RuntimeDetour.HookGen.exe", "LICENSCE.md");
                EditorUtility.SetDirty(hookGenerator);
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        public override void Cleanup()
        {
            const string pluginPath = "Packages/fortunesteam-fortunesfromthescrapyard/FortunesFromTheScrapyard.dll";
            // Rebuild MonoScript entries now that their game-dependent base classes can resolve.
            AssetDatabase.ImportAsset(pluginPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            if (!AssetDatabase.LoadAllAssetsAtPath(pluginPath).OfType<MonoScript>()
                .Any(script => script.name == "ExtendedAssetCollection" && script.GetClass() != null))
                throw new InvalidOperationException("Fortunes asset collection scripts did not resolve after importing the game.");
        }

        private static UnityEngine.Object CopyTool(string packageRoot, string directory, string executable, string license)
        {
            string sourceDirectory = Path.Combine(packageRoot, "Binary", directory);
            string destinationDirectory = ToolsPath + "/" + directory;
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(Path.Combine(sourceDirectory, license), Path.Combine(destinationDirectory, license), true);

            // The upstream processors use Path.GetFullPath on asset paths, which cannot resolve virtual UPM paths.
            string assetPath = destinationDirectory + "/" + executable;
            File.Copy(Path.Combine(sourceDirectory, executable), assetPath, true);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var tool = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (!tool)
                throw new InvalidOperationException("Unity could not import the executable at " + assetPath);
            return tool;
        }
    }
}
