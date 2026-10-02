using System.IO;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;

namespace Fortunes.Editor
{
    [InitializeOnLoad]
    public static class GameAddressablesPath
    {
        static GameAddressablesPath()
        {
            var settings = ThunderKitSetting.GetOrCreateSettings<ThunderKitSettings>();
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
