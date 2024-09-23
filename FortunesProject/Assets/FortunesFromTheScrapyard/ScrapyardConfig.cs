using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortunesFromTheScrapyard
{
    public class ScrapyardConfig
    {
        public const string PREFIX = "FortunesFromTheScrapyard.";
        public const string ID_MAIN = PREFIX + "Main";
        public const string ID_EQUIPS = PREFIX + "Equips";
        public const string ID_ITEMS = PREFIX + "Items";
        public const string ID_ELITES = PREFIX + "Elites";
        public const string ID_SURVIVORS = PREFIX + "Survivors";

        internal static ConfigFactory configFactory { get; private set; }
        public static ConfigFile configMain { get; private set; }
        public static ConfigFile configEquips { get; private set; }
        public static ConfigFile configItems { get; private set; }

        public static ConfigFile configElites { get; private set; }

        public static ConfigFile configSurvivors { get; private set; }

        internal static IEnumerator RegisterToModSettingsManager()
        {
            yield break;
        }

        internal ScrapyardConfig(BaseUnityPlugin bup)
        {
            configFactory = new ConfigFactory(bup, true);
            configMain = configFactory.CreateConfigFile(ID_MAIN, true);
            configEquips = configFactory.CreateConfigFile(ID_EQUIPS, true);
            configItems = configFactory.CreateConfigFile(ID_ITEMS, true);
            configElites = configFactory.CreateConfigFile(ID_ELITES, true);
            configSurvivors = configFactory.CreateConfigFile(ID_SURVIVORS, true);
        }
    }
}