using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using EntityStates;
using SearchableAttribute = HG.Reflection.SearchableAttribute;
using ShaderSwapper;
using MonoMod.RuntimeDetour;
using R2API;
using BepInEx.Bootstrap;
using R2API.Networking;
using MSU;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: SearchableAttribute.OptIn]
[module: UnverifiableCode]

namespace FortunesFromTheScrapyard
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [BepInDependency(MSUMain.GUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInDependency(ExecuteAPI.PluginGUID, "1.1.1")]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    public class FFTSMain : BaseUnityPlugin
    {
        public const string MODUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        public const string MODNAME = "Fortunes From the Scrapyard";
        public const string MODVERSION = "1.0.0";

        public static FFTSMain instance;

        public static bool emotesInstalled => Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");

        void Awake()
        {
            instance = this;

            new FFTSLog(Logger);
            new FFTSConfig(this);

            //NetworkingAPI.RegisterMessageType<SyncTime>();

            new FFTSContent();

            LanguageFileLoader.AddLanguageFilesFromMod(this, "languages");
        }

        private void Start()
        {
            FFTSSoundbank.Init();
            AddHooks();
        }

        private void OnDestroy()
        {
            if (emotesInstalled)
                Survivors.Cloaker.CloakerEmotes.Uninstall();
        }

        private void AddHooks()
        {

        }
    }
}
