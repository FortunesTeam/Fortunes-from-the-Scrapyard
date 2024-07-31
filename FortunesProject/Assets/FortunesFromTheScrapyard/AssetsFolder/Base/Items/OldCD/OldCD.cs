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
        [FormatToken(TOKEN, 0)]
        public static float cooldownReduction = 10f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static float cooldownReductionStack = 10f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool weezerEnabled = true;

        public static GameObject WeezerEffect;
        public override void Initialize()
        {
            WeezerEffect = AssetCollection.FindAsset<GameObject>("WeezerEffect");

            On.RoR2.GenericSkill.OnExecute += GenericSkill_OnExecute;
        }

        private void GenericSkill_OnExecute(On.RoR2.GenericSkill.orig_OnExecute orig, GenericSkill skillSlot)
        {
            orig.Invoke(skillSlot);

            if(skillSlot.characterBody.HasItem(ScrapyardContent.Items.OldCD))
            {

                GenericSkill primary = skillSlot.characterBody.skillLocator.primary;
                GenericSkill secondary = skillSlot.characterBody.skillLocator.secondary;
                GenericSkill utility = skillSlot.characterBody.skillLocator.utility;
                GenericSkill special = skillSlot.characterBody.skillLocator.special;
                bool meetsCDRequirement = skillSlot.baseRechargeInterval >= 0.5f && skillSlot.rechargeStock > 0;

                if(meetsCDRequirement)
                {
                    float refund = Util.ConvertAmplificationPercentageIntoReductionPercentage(GetStackValue(cooldownReduction, cooldownReductionStack, skillSlot.characterBody.GetItemCount(ScrapyardContent.Items.OldCD))) / skillSlot.rechargeStock;
                    if (skillSlot != primary) primary.rechargeStopwatch += primary.cooldownRemaining * (refund / 100f);
                    if (skillSlot != secondary) secondary.rechargeStopwatch += secondary.cooldownRemaining * (refund / 100f);
                    if (skillSlot != utility) utility.rechargeStopwatch += utility.cooldownRemaining * (refund / 100f);
                    if (skillSlot != special) special.rechargeStopwatch += special.cooldownRemaining * (refund / 100f);

                    if(weezerEnabled)
                    {
                        int randomSFX;
                        if (skillSlot == primary)
                        {
                            randomSFX = UnityEngine.Random.Range(0, 1);

                            switch (randomSFX)
                            {
                                case 0:
                                    {
                                        Util.PlaySound("sfx_cd_1", skillSlot.gameObject);
                                        break;
                                    }
                                case 1:
                                    {
                                        Util.PlaySound("sfx_cd_2", skillSlot.gameObject);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (skillSlot == secondary) Util.PlaySound("sfx_cd_1", skillSlot.gameObject);
                            if (skillSlot == utility) Util.PlaySound("sfx_cd_2", skillSlot.gameObject);
                            if (skillSlot == special) Util.PlaySound("sfx_cd_3", skillSlot.gameObject);
                        }
                    }
                    
                    EffectManager.SimpleImpactEffect(WeezerEffect, skillSlot.characterBody.corePosition, Vector3.up, transmit: true);
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
