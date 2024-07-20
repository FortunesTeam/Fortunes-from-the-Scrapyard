using System;
using FortunesFromTheScrapyard.Modules;
using FortunesFromTheScrapyard.Items;
using FortunesFromTheScrapyard.Equipment;
using UnityEngine.UIElements;

namespace FortunesFromTheScrapyard.Items.Content
{
    public static class FortunesItemTokens
    {
        public static void Init()
        {
            AddFortunesItemTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("FortunesItem.txt");
            //todo guide
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddFortunesItemTokens()
        {   
            string prefix = "FORTUNES";

            string item = "";

            string currentItem = prefix + item;
            
            #region Tier 1 Items
            item = "_SPRAYCAN_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Spray Can");
            Language.Add(currentItem + "PICKUP", "Outdated.");
            Language.Add(currentItem + "DESC", $"Outdated.");
            Language.Add(currentItem + "LORE", "ow");
            #endregion

            #region Tier2 Items

            item = "_TAKEOUT_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Takeout");
            Language.Add(currentItem + "PICKUP", "Gain a random stat buff for the rest of the stage.");
            Language.Add(currentItem + "DESC", $"On the {Tokens.UtilityText("beginning of each stage")}, " +
            $"gain a random {Tokens.DamageText("damage")}, {Tokens.UtilityText("movement speed")}, or {Tokens.HealingText("base health regeneration")} buff " +
            $"until the stage ends. " +
            $"Can increase base damage by {Tokens.DamageText(Tokens.ConvertDecimal(Takeout.damageBase))} {Tokens.StackText("+" + Tokens.ConvertDecimal(Takeout.damageStack))}, " +
            $"movement speed by {Tokens.UtilityText(Tokens.ConvertDecimal(Takeout.mspdBase))} {Tokens.StackText("+" + Tokens.ConvertDecimal(Takeout.mspdStack))}, " +
            $"or base health regeneration by {Tokens.HealingText(Takeout.regenBase.ToString() + " hp/s")} {Tokens.StackText("+" + Takeout.regenStack.ToString() + " hp/s")}");
            Language.Add(currentItem + "LORE", "ow");

            item = "_HEADPHONES_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Broken Headphones");
            Language.Add(currentItem + "PICKUP", "Chance to disorient enemies.");
            Language.Add(currentItem + "DESC", $"Outdated.");
            Language.Add(currentItem + "LORE", "ow");

            item = "_MULTITOOL_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "MultiTool");
            Language.Add(currentItem + "PICKUP", "The next interactable triggers an additional time. Regenerates every stage.");
            Language.Add(currentItem + "DESC", $"The next interactable triggers an additional time. Regenerates every stage.");
            Language.Add(currentItem + "LORE", "ow");

            item = "_CONSUMED_MULTITOOL_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "MultiTool");
            Language.Add(currentItem + "PICKUP", "It is no longer useful. Regenerates next stage.");
            Language.Add(currentItem + "DESC", $"It is no longer useful. Regenerates next stage.");
            Language.Add(currentItem + "LORE", "ow");
            #endregion

            #region Tier3 Items
            
            item = "_INJECTION_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Lethal Injection");
            Language.Add(currentItem + "PICKUP", "High damage afflicts permanent execute.");
            Language.Add(currentItem + "DESC", $"Hits that deal {Tokens.DamageText("more than 400% damage")} also inflict a toxin " +
            $"that {Tokens.HealthText("permanently")} reduces maximum health by {Tokens.DamageText("1%")} every {LethalInjection.toxinInterval} seconds. " +
            $"Toxin lasts for {Tokens.UtilityText((LethalInjection.toxinDurationBase * LethalInjection.toxinInterval).ToString())} seconds " +
            $"{Tokens.StackText("+" + (LethalInjection.toxinDurationStack * LethalInjection.toxinInterval).ToString())}.");
            Language.Add(currentItem + "LORE", "ow");

            #endregion
            
            #region Boss Items
            #endregion
            
            #region Lunar Items
            item = "_LUNARMONEY_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Counterfeit Currency");
            Language.Add(currentItem + "PICKUP", "Begin each stage with extra gold... " + Tokens.HealthText("BUT gain less gold."));
            Language.Add(currentItem + "DESC", $"Begin each stage with {Tokens.DamageText("$" + CounterfeitCurrency.freeMoneyBase.ToString() + " gold")} " +
            $"{Tokens.StackText("+$" + CounterfeitCurrency.freeMoneyStack.ToString())}. {Tokens.UtilityText("Scales over time.")} " +
            $"{Tokens.HealthText("ALL other sources of income")} are reduced by {Tokens.HealthText("-" + Tokens.ConvertDecimal(CounterfeitCurrency.incomePenaltyBase))} " +
            $"{Tokens.StackText("-" + Tokens.ConvertDecimal(CounterfeitCurrency.incomePenaltyStack))}.");
            Language.Add(currentItem + "LORE", "ow");
            #endregion

            #region Void Items
            #endregion

            #region Prismatic Items
            #endregion

            #region Equipment
            item = "_ENERGY_";
            currentItem = prefix + item;
            Language.Add(currentItem + "NAME", "Energy Bar");
            Language.Add(currentItem + "PICKUP", "Restore 25% of your max health, being recharing shields and gain a brief speed boost.");
            Language.Add(currentItem + "DESC", $"Restore {Tokens.HealingText(Tokens.ConvertDecimal(EnergyBar.healAmount) + " max health")}, begin recharging {Tokens.HealingText("shield")}, " +
            $",and increase {Tokens.UtilityText("movement speed")} by {Tokens.UtilityText(Tokens.ConvertDecimal(EnergyBar.speedBonus))} " +
            $"for {Tokens.UtilityText(EnergyBar.speedBonusDuration.ToString())} seconds.");
            Language.Add(currentItem + "LORE", "ow");
            #endregion
        }
    }
}