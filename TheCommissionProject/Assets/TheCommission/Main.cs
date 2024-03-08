using BepInEx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheCommission
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string GUID = "com.RoR2CommissionTeam.TheCommission";
        public const string MODNAME = "TheCommission";
        public const string VERSION = "0.0.1";

        public static Main Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            new TCLog(Logger);
            new CommissionContent();
        }
    }
}