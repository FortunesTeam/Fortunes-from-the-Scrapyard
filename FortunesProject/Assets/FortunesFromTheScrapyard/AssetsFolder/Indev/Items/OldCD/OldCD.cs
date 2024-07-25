using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    public class OldCD : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_OLDCD_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float cooldownReduction = 0.025f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float cooldownReductionStack = 0.025f;

        public override void Initialize()
        {
            On.RoR2.GenericSkill.OnExecute += GenericSkill_OnExecute;
        }

        private void GenericSkill_OnExecute(On.RoR2.GenericSkill.orig_OnExecute orig, GenericSkill skillSlot)
        {
            orig.Invoke(skillSlot);

            if(skillSlot.characterBody.HasItem(ScrapyardContent.Items.OldCD))
            {
                float refund = GetStackValue(cooldownReduction, cooldownReductionStack, skillSlot.characterBody.GetItemCount(ScrapyardContent.Items.OldCD));
                GenericSkill primary = skillSlot.characterBody.skillLocator.primary;
                GenericSkill secondary = skillSlot.characterBody.skillLocator.secondary;
                GenericSkill utility = skillSlot.characterBody.skillLocator.utility;
                GenericSkill special = skillSlot.characterBody.skillLocator.special;
                bool primaryIsJartificer = skillSlot.baseRechargeInterval > 0.5f;

                if(primaryIsJartificer)
                {
                    if (skillSlot != primary) primary.rechargeStopwatch += primary.cooldownRemaining * refund;
                    if (skillSlot != secondary) secondary.rechargeStopwatch += secondary.cooldownRemaining * refund;
                    if (skillSlot != utility) utility.rechargeStopwatch += utility.cooldownRemaining * refund;
                    if (skillSlot != special) special.rechargeStopwatch += special.cooldownRemaining * refund;
                }
            }
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acOldCD", ScrapyardBundle.Items);
        }
    }
}
