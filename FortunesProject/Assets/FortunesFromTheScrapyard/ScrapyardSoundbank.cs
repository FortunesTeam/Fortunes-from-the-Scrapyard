using RoR2;
using UnityEngine;
using Path = System.IO.Path;

namespace FortunesFromTheScrapyard
{
    public static class ScrapyardSoundbank
    {
        private static bool initialized = false;
        public static string SoundBankDirectory
        {
            get
            {
                return Path.Combine(Path.Combine(Path.GetDirectoryName(ScrapyardMain.instance.Info.Location)), "soundbanks");
            }
        }

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            //LogCore.LogE(AkSoundEngine.ClearBanks().ToString());
            AKRESULT akResult = AkSoundEngine.AddBasePath(SoundBankDirectory);
            AkSoundEngine.LoadBank("FortunesBank", out _);
        }
        /*
        [SystemInitializer(dependencies: typeof(MusicTrackCatalog))]
        public static void MusicInit()
        {
            AkSoundEngine.LoadBank("ScrapyardMusic", -1, out var bank);
            GameObject.Instantiate(ScrapyardAssets.LoadAsset<GameObject>("ScrapyardMusicInitializer", ScrapyardBundle.Base));
        }
        */
    }
}
