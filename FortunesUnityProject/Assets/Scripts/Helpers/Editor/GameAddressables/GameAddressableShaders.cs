#if TK_ADDRESSABLE
using ThunderKit.Addressable.Tools;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace Fortunes.Editor
{
    [InitializeOnLoad]
    internal static class GameAddressableShaders
    {
        static GameAddressableShaders()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                var settings = ThunderKitSetting.GetOrCreateSettings<AddressableGraphicsSettings>();
                ReleaseShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
                ReleaseShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
                ReleaseShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += RestoreShaders;
            }
        }

        private static void ReleaseShader(string address, BuiltinShaderType type)
        {
            if (string.IsNullOrEmpty(address) || GraphicsSettings.GetShaderMode(type) != BuiltinShaderMode.UseCustom)
                return;

            Shader shader = GraphicsSettings.GetCustomShader(type);
            GraphicsSettings.SetCustomShader(type, null);
            GraphicsSettings.SetShaderMode(type, BuiltinShaderMode.UseBuiltin);

            // Release ThunderKit's load before Unity destroys the shader, avoiding a stale cached result.
            if (shader)
                Addressables.Release(shader);
        }

        private static void RestoreShaders()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var settings = ThunderKitSetting.GetOrCreateSettings<AddressableGraphicsSettings>();
            AddressableGraphicsSettings.SetShader(settings.CustomDeferredShading, BuiltinShaderType.DeferredShading);
            AddressableGraphicsSettings.SetShader(settings.CustomDeferredReflection, BuiltinShaderType.DeferredReflections);
            AddressableGraphicsSettings.SetShader(settings.CustomDeferredScreenspaceShadows, BuiltinShaderType.ScreenSpaceShadows);
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
#endif
