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
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [BepInDependency("com.Moffein.AccurateEnemies", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.RiskyArtifacts", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("HIFU.Inferno", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("EarthZebra.someshitidk", BepInDependency.DependencyFlags.SoftDependency)]
    public class FFTSMain : BaseUnityPlugin
    {
        public const string MODUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        public const string MODNAME = "Fortunes From the Scrapyard";
        public const string MODVERSION = "0.0.1";

        //public const string DEVELOPER_PREFIX = "MYSTICAL";

        public static FFTSMain instance;

        public static bool emotesInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
        public static bool scepterInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
        public static bool InfernoInstalled => Chainloader.PluginInfos.ContainsKey("HIFU.Inferno");
        public static bool RiskyArtifactsInstalled => Chainloader.PluginInfos.ContainsKey("com.Moffein.RiskyArtifacts");
        public static bool NeuromancerInstalled => Chainloader.PluginInfos.ContainsKey("EarthZebra.someshitidk");

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

        private void AddHooks()
        {

        }
    }
}
