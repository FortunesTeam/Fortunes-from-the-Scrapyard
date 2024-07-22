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
    public class Takeout : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_TAKEOUT_DESC";
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageBase = 0.25f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageStack = 0.25f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float mspdBase = 0.25f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float mspdStack = 0.25f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float regenBase = 3f;
        [FormatToken(TOKEN, 5)]
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float regenStack = 3f;

        private static WeightedSelection<BuffDef> _weightedBuffSelection = new WeightedSelection<BuffDef>();
        public override void Initialize()
        {
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutDmg"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutSpeed"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutRegen"), 10);
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
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acTakeout", ScrapyardBundle.Items);
        }


        public class TakeoutBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.Takeout;

            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if(body.HasBuff(ScrapyardContent.Buffs.bdTakeoutDmg))
                {
                    args.damageMultAdd += GetStackValue(damageBase, damageStack, stack);
                }
                else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutSpeed))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, stack);
                }
                else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutRegen))
                {
                    args.baseRegenAdd += GetStackValue(regenBase, regenStack, stack) * (1 + 0.2f * body.level);
                }
            }

            private void Start()
            {
                if (!NetworkServer.active)
                    return;

                var buffDef = _weightedBuffSelection.Evaluate(Random.value);
                body.AddBuff(buffDef);
            }

            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutDmg))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutDmg);
                    }
                    else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutSpeed))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutSpeed);
                    }
                    else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutRegen))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutRegen);
                    }
                }
            }
        }
    }
}
