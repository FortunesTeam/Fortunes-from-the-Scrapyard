using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using FortunesFromTheScrapyard.Equipment;
using FortunesFromTheScrapyard.Items;
using FortunesFromTheScrapyard.Items.Content;
using FortunesFromTheScrapyard.Modules;
using FortunesFromTheScrapyard.Skills;
using R2API;
using R2API.Utils;
using UnityEngine;
using R2API.Networking;

namespace FortunesFromTheScrapyard
{
    [BepInDependency("com.bepis.r2api.dot")]
    [BepInDependency("com.bepis.r2api.networking")]
    [BepInDependency("com.bepis.r2api.prefab")]
    [BepInDependency("com.bepis.r2api.difficulty")]
    [BepInDependency("com.bepis.r2api.tempvisualeffect")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency(NetworkingAPI.PluginGUID)]
    public class FortunesPlugin : BaseUnityPlugin
    {
        internal const string GUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        internal const string MODNAME = "FortunesFromTheScrapYard";
        internal const string VERSION = "0.5.0";
        internal static FortunesPlugin instance { get; private set; }
        void Awake()
        {
            instance = this;

            FortunesAssets.PopulateAssets();
            
            Modules.Config.Init();
            Modules.Language.Init();
            Modules.Hooks.Init();
            
            ConfigManager.HandleConfigAttributes(GetType(), "Fortunes", Modules.Config.MyConfig);

            FortunesItemTokens.Init();

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            //new Survivor().Init();
            /*BeginInitializing<SurvivorBase>(allTypes);
            Modules.Language.TryPrintOutput("FortunesSurvivors.txt");

            BeginInitializing<SkillBase>(allTypes);
            Modules.Language.TryPrintOutput("FortunesSkills.txt");*/

            //Survivor.instance.InitializeCharacterMaster();
            
            FortunesContent.Initialize();

            BeginInitializingItems<ItemBase>(allTypes);

            BeginInitializingEquipment<EquipmentBase>(allTypes);
        }

        private void BeginInitializingItems<T>(Type[] allTypes) where T : ItemBase
        {
            Type baseType = typeof(T);
            if (!baseType.IsAbstract)
            {
                return;
            }

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            foreach (var objType in objTypesOfBaseType)
            {
                T obj = (T)System.Activator.CreateInstance(objType);
                InitializeBaseType(obj as ItemBase);
            }
        }    
        void InitializeBaseType(ItemBase obj)
        {
            obj.Init();
        }

        private void BeginInitializingEquipment<T>(Type[] allTypes) where T : EquipmentBase
        {
            Type baseType = typeof(T);
            if (!baseType.IsAbstract)
            {
                return;
            }

            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            foreach (var objType in objTypesOfBaseType)
            {
                T obj = (T)System.Activator.CreateInstance(objType);
                InitializeEquipmentBaseType(obj as EquipmentBase);
            }
        }    
        void InitializeEquipmentBaseType(EquipmentBase obj)
        {
            obj.Init();
        }
    }
}
