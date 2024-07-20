using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    public class Takeout : ItemBase<Takeout>
    {
        [AutoConfig("Base Damage Bonus", 0.25f)]
        public static float damageBase = 0.25f;
        [AutoConfig("Stacking Damage Bonus", 0.25f)]
        public static float damageStack = 0.25f;

        [AutoConfig("Base Movement Speed Bonus", 0.25f)]
        public static float mspdBase = 0.25f;
        [AutoConfig("Stacking Movement Speed Bonus", 0.25f)]
        public static float mspdStack = 0.25f;

        [AutoConfig("Base Regen Bonus", 3f)]
        public static float regenBase = 3f;
        [AutoConfig("Stacking Regen Bonus", 3f)]
        public static float regenStack = 3f;
        public override void Init()
        {
            itemName = "Takeout";
            base.Init();
        }
        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            GetStatCoefficients += TakeoutStats;
        }

        private void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.master)
                {
                    int itemCount = GetCount(self);

                    self.AddItemBehavior<TakeoutBehavior>(itemCount);
                }
            }
        }

        private void TakeoutStats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = GetCount(sender);
            if(itemCount > 0)
            {
                if (sender.HasBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutDmg")))
                {
                    args.damageMultAdd += GetStackValue(damageBase, damageStack, itemCount);
                }
                else if (sender.HasBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutSpeed")))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, itemCount);
                }
                else if (sender.HasBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutRegen")))
                {
                    args.baseRegenAdd += GetStackValue(regenBase, regenStack, itemCount) * (1 + 0.2f * sender.level);
                }
            }
        }
    }

    public class TakeoutBehavior : CharacterBody.ItemBehavior
    {
        private void Start()
        {
            if (body)
            {
                if (Util.CheckRoll(100 / 3))
                {
                    body.AddBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutDmg"));
                    return;
                }

                if (Util.CheckRoll(100 / 2))
                {
                    body.AddBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutSpeed"));
                    return;
                }
                
                body.AddBuff(FortunesContent.contentPack.buffDefs.Find("TakeoutRegen"));
            }
        }
    }
}
