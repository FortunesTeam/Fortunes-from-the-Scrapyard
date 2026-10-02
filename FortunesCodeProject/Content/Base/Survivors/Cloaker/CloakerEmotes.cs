using System.Runtime.CompilerServices;
using EmotesAPI;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    internal static class CloakerEmotes
    {
        private static bool initialized;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Initialize(GameObject body, GameObject skeleton)
        {
            if (initialized) return;
            CustomEmotesAPI.ImportArmature(body, skeleton);
            CustomEmotesAPI.animChanged += OnAnimationChanged;
            initialized = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void OnAnimationChanged(string animation, BoneMapper mapper)
        {
            if (mapper && mapper.mapperBody && mapper.mapperBody.TryGetComponent(out CloakerController controller))
                controller.SetEmoting(animation != "none");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Uninstall()
        {
            CustomEmotesAPI.animChanged -= OnAnimationChanged;
            initialized = false;
        }
    }
}
