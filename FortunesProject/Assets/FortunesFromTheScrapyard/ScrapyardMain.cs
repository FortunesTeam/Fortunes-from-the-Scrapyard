using BepInEx;
using MSU;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace FortunesFromTheScrapyard
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ScrapyardMain : BaseUnityPlugin
    {
        public const string GUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        public const string VERSION = "0.0.1";
        public const string NAME = "Fortunes From the Scrapyard";

        //Singleton access pattern to our instance.
        internal static ScrapyardMain instance { get; private set; }

        private void Awake()
        {
            instance = this;

            new ScrapyardLog(Logger);
            new ScrapyardConfig(this);

            //We do not load our assetbundles or content at awake, instead, we create a new instance of this class,
            //which implements the game's IContentPackProvider interface.
            new ScrapyardContent();

            LanguageFileLoader.AddLanguageFilesFromMod(this, "ScrapyardLanguage");
        }
    }
}