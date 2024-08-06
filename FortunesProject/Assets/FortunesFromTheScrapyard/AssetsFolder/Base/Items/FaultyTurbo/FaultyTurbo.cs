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
    public class FaultyTurbo : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_FAULTYTURBO_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float movespeedBonus = 0.8f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static float baseDuration = 5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 2)]
        public static float baseDurationStack = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float checkInterval = 1f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float baseChance = 15f;
        [FormatToken(TOKEN, 5)]
        public static float chancePerStack = 15f;

        public override void Initialize()
        {
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
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acFaultyTurbo", ScrapyardBundle.Items);
        }

        public class FaultyTurboBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.FaultyTurbo;

            private float timer = 0f;

            private float sfxCooldown = baseDuration;
            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo))
                {
                    args.moveSpeedMultAdd += movespeedBonus;
                }
            }

            private void FixedUpdate()
            {
                sfxCooldown += Time.fixedDeltaTime;
                if (base.body.isSprinting) timer += Time.fixedDeltaTime;
                else timer = 0f;

                if (timer >= checkInterval)
                {
                    timer = 0f;
                    if (Util.CheckRoll(GetStackValue(baseChance, chancePerStack, stack) + Util.ConvertAmplificationPercentageIntoReductionPercentage(baseChance), body.master))
                    {
                        if(NetworkServer.active)
                        {
                            if (body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo)) body.RemoveOldestTimedBuff(ScrapyardContent.Buffs.bdFaultyTurbo);
                            body.AddTimedBuff(ScrapyardContent.Buffs.bdFaultyTurbo, GetStackValue(baseDuration, baseDurationStack, stack));

                            if (sfxCooldown >= GetStackValue(baseDuration, baseDurationStack, stack))
                            {
                                Util.PlaySound("sfx_turbo_start", body.gameObject);
                                sfxCooldown = 0f;
                            }
                        }
                    }
                }
            }

            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    if(body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdFaultyTurbo);
                    }
                }
            }
        }
    }
}
