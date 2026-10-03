using System;
using System.IO;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;

namespace Fortunes.Editor
{
    public sealed class GameAddressablesPath : AssetPostprocessor
    {
        private const string SettingsPath = "Assets/ThunderKitSettings/ThunderKitSettings.asset";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!didDomainReload && Array.IndexOf(importedAssets, SettingsPath) < 0 &&
                Array.IndexOf(movedAssets, SettingsPath) < 0)
                return;

            // Before asset import, GetOrCreateSettings can overwrite settings that are not yet loadable.
            var settings = AssetDatabase.LoadAssetAtPath<ThunderKitSettings>(SettingsPath);
            if (!settings)
            {
                Debug.LogError("Fortunes cannot load ThunderKit Settings at " + SettingsPath);
                return;
            }

            string path = settings.AddressableAssetsPath;
            if (!File.Exists(Path.Combine(path, "catalog.json")))
            {
                Debug.LogError("Fortunes cannot locate the game Addressables catalog. Check the game path in ThunderKit Settings: " + path);
                return;
            }

            // Bundle loading and editor save hooks can run before TK_ADDRESSABLE is restored.
            AddressablesRuntimeProperties.SetPropertyValue(
                "UnityEngine.AddressableAssets.Addressables.RuntimePath", path.Replace('\\', '/'));
        }
    }
}
