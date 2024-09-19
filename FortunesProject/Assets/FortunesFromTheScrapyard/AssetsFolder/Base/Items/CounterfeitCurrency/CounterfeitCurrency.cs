
using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using MSU.Config;
using MSU;

namespace FortunesFromTheScrapyard.Items
{
    public class CounterfeitCurrency : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_COUNTERFEITCURRENCY_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        public static int freeMoneyBase = 50;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static int freeMoneyStack = 50;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float incomePenaltyBase = 0.2f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float incomePenaltyStack = 0.2f;

        public override void Initialize()
        {
            On.RoR2.CharacterMaster.GiveMoney += CounterfeitPenalty;
            Inventory.onServerItemGiven += CounterfeitPickupReward;
            On.RoR2.CharacterMaster.OnBodyStart += CounterfeitStartReward;
        }

        private void CounterfeitStartReward(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            int itemCount = body.inventory.GetItemCount(itemDef);
            if (itemCount > 0)
            {
                float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, itemCount);
                float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(itemCount);
                self.GiveMoney((uint)Run.instance.GetDifficultyScaledCost((int)freeMoneyCompensated, Run.instance.difficultyCoefficient));
                Util.PlaySound("sfx_lunarmoney_start", body.gameObject);    
            }
            orig(self, body);
        }

        private void CounterfeitPickupReward(Inventory inv, ItemIndex itemIndex, int count)
        {
            if (itemIndex == itemDef.itemIndex)
            {
                CharacterMaster master = inv.gameObject.GetComponent<CharacterMaster>();
                if (master)
                {
                    float freeMoney = GetStackValue(freeMoneyBase, freeMoneyStack, count);
                    float freeMoneyCompensated = freeMoney / CalculateIncomeModifier(inv.GetItemCount(itemDef));
                    master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost((int)freeMoneyCompensated, Stage.instance.entryDifficultyCoefficient));
                }
            }
        }

        private void CounterfeitPenalty(On.RoR2.CharacterMaster.orig_GiveMoney orig, RoR2.CharacterMaster self, uint amount)
        {
            int itemCount = self.inventory.GetItemCount(itemDef);
            amount = (uint)Mathf.Min(amount * CalculateIncomeModifier(itemCount), amount);

            orig(self, amount);
        }

        public static float CalculateIncomeModifier(int itemCount)
        {
            float incomeModifier = 1;
            if (itemCount > 0)
            {
                incomeModifier *= (1 - incomePenaltyBase);
                incomeModifier *= Mathf.Pow(1 - incomePenaltyStack, itemCount - 1);
            }
            return incomeModifier;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acCounterfeitCurrency", ScrapyardBundle.Items);
        }
    }
}
