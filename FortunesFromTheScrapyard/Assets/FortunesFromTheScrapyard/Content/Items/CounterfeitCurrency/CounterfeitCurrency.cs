using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace FortunesFromTheScrapyard.Items
{
    public class CounterfeitCurrency : ItemBase
    {
        [AutoConfig("Free Money Base", 50)]
        public static int freeMoneyBase = 50;
        [AutoConfig("Free Money Stack", 50)]
        public static int freeMoneyStack = 50;
        [AutoConfig("Income Penalty Base", 0.25f)]
        public static float incomePenaltyBase = 0.25f;
        [AutoConfig("Income Penalty Stack", 0.25f)]
        public static float incomePenaltyStack = 0.25f;
        public override void Init()
        {
            itemName = "CounterfeitCurrency";
            base.Init();
        }
        public override void Hooks()
        {
            On.RoR2.CharacterMaster.GiveMoney += CounterfeitPenalty;
            Inventory.onServerItemGiven += CounterfeitPickupReward;
            On.RoR2.CharacterMaster.OnBodyStart += CounterfeitStartReward;
        }

        private void CounterfeitStartReward(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            int itemCount = body.inventory.GetItemCount(GetItemDef());
            if(itemCount > 0)
            {
                float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, itemCount);
                float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(itemCount);
                self.GiveMoney((uint)Run.instance.GetDifficultyScaledCost((int)freeMoneyCompensated, Run.instance.difficultyCoefficient));
            }
            orig(self, body);
        }

        private void CounterfeitPickupReward(Inventory inv, ItemIndex itemIndex, int count)
        {
            if(itemIndex == GetItemDef().itemIndex)
            {
                CharacterMaster master = inv.gameObject.GetComponent<CharacterMaster>();
                if (master)
                {
                    float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, count);
                    float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(GetCount(master));
                    master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost((int)freeMoneyCompensated, Stage.instance.entryDifficultyCoefficient));
                }
            }
        }

        private void CounterfeitPenalty(On.RoR2.CharacterMaster.orig_GiveMoney orig, RoR2.CharacterMaster self, uint amount)
        {
            int itemCount = GetCount(self);
            amount = (uint)Mathf.Min(amount * CalculateIncomeModifier(itemCount), amount);

            orig(self, amount);
        }

        public static float CalculateIncomeModifier(int itemCount)
        {
            float incomeModifier = 1;
            if(itemCount > 0)
            {
                incomeModifier *= (1 - incomePenaltyBase);
                incomeModifier *= Mathf.Pow(1 - incomePenaltyStack, itemCount - 1);
                Debug.Log(incomeModifier);
            }
            return incomeModifier;
        }
    }
}
