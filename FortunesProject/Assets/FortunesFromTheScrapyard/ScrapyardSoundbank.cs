using RoR2;
using UnityEngine;
using Path = System.IO.Path;
using MonoMod.RuntimeDetour;
using System;
using R2API.Utils;
using R2API;

namespace FortunesFromTheScrapyard
{
    public static class ScrapyardSoundBank

    {
        public static string soundBankDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(ScrapyardMain.instance.Info.Location), "soundbanks");
            }
        }

        public static void Init()
        {
            var hook = new Hook(
            typeof(AkSoundEngineInitialization).GetMethodCached(nameof(AkSoundEngineInitialization.InitializeSoundEngine)),
            typeof(ScrapyardSoundBank).GetMethodCached(nameof(AddBanks)));


        }

        private static bool AddBanks(Func<AkSoundEngineInitialization, bool> orig, AkSoundEngineInitialization self)
        {
            var res = orig(self);

            LoadBanks();

            return res;
        }

        private static void LoadBanks()
        {
            //LogCore.LogE(AkSoundEngine.ClearBanks().ToString());
            AkSoundEngine.AddBasePath(soundBankDirectory);
            AkSoundEngine.LoadBank("FortunesBank", /*-1,*/ out var bank);
        }

        [SystemInitializer(dependencies: typeof(MusicTrackCatalog))]
        public static void MusicInit()
        {
            //AkSoundEngine.LoadBank("scrapyardmusic", /*-1,*/ out var bank);
        }
    }
}
