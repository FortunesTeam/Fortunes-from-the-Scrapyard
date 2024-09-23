using BepInEx;
using MSU;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using R2API.Networking;
using FortunesFromTheScrapyard.Elite;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace FortunesFromTheScrapyard
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.Moffein.AccurateEnemies", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    public class ScrapyardMain : BaseUnityPlugin
    {
        public const string GUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        public const string VERSION = "0.0.1";
        public const string NAME = "Fortunes From the Scrapyard";
        //Singleton access pattern to our instance.
        internal static ScrapyardMain instance { get; private set; }
        public static bool emotesInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
        public static bool scepterInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");

        private void Awake()
        {
            instance = this;
            new ScrapyardLog(Logger);
            new ScrapyardConfig(this);

            NetworkingAPI.RegisterMessageType<SyncTime>();
            NetworkingAPI.RegisterMessageType<NetworkEquipmentSelection.SyncDisplay>();

            //We do not load our assetbundles or content at awake, instead, we create a new instance of this class,
            //which implements the game's IContentPackProvider interface.
            new ScrapyardContent();

            LanguageFileLoader.AddLanguageFilesFromMod(this, "languages");
        }

        private void Start()
        {
            ScrapyardSoundBank.Init();
        }
    }
}