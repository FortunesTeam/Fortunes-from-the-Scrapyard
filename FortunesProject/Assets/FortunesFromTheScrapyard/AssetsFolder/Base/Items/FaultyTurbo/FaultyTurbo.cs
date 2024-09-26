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
        [FormatToken(TOKEN, 0)]
        public static int maxStacks = 3;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static int maxStacksPerStack = 3;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float movespeedBonusPerInterval = 0.1f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float movespeedBonusPerIntervalStack = 0.1f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float checkInterval = 1f;

        public override void Initialize()
        {
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
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
            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo))
                {
                    args.moveSpeedMultAdd += GetStackValue(movespeedBonusPerInterval, movespeedBonusPerIntervalStack, stack) * body.GetBuffCount(ScrapyardContent.Buffs.bdFaultyTurbo);
                }
            }

            private void FixedUpdate()
            {
                bool atMaxStacks = body.GetBuffCount(ScrapyardContent.Buffs.bdFaultyTurbo) >= GetStackValue(maxStacks, maxStacksPerStack, stack);
                
                if (base.body.isSprinting && !atMaxStacks)
                {
                    timer += Time.fixedDeltaTime;
                }
                else if(body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo)) timer -= Time.fixedDeltaTime;

                if (timer >= checkInterval && !atMaxStacks)
                {
                    timer = 0f;
                    if(NetworkServer.active)
                    {
                        body.AddBuff(ScrapyardContent.Buffs.bdFaultyTurbo);
                    }

                    Util.PlaySound("sfx_turbo_start", base.gameObject);
                }
                else if(timer < 0f && body.HasBuff(ScrapyardContent.Buffs.bdFaultyTurbo))
                {
                    timer = checkInterval / 2f;
                    body.RemoveBuff(ScrapyardContent.Buffs.bdFaultyTurbo);
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
