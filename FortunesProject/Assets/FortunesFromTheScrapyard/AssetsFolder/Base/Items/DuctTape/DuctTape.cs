﻿using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    public class DuctTape : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_TAPE_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float healthThreshold = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float baseHealingIncrease = 0.15f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float healingIncreasePerStack = 0.15f;

        public override void Initialize()
        {
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }
        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if (self.body.HasBuff(ScrapyardContent.Buffs.bdDuctTape))
            {
                amount += amount * GetStackValue(baseHealingIncrease, healingIncreasePerStack, self.body.GetItemCount(ScrapyardContent.Items.DuctTape));
            }
            return orig.Invoke(self, amount, procChainMask, nonRegen);
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
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acDuctTape", ScrapyardBundle.Items);
        }
        public class DuctTapeBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.DuctTape;

            private void FixedUpdate()
            {
                if (body.healthComponent.health <= body.healthComponent.fullCombinedHealth * healthThreshold)
                {
                    if (!body.HasBuff(ScrapyardContent.Buffs.bdDuctTape))
                    {
                        if(NetworkServer.active) body.AddBuff(ScrapyardContent.Buffs.bdDuctTape);
                        Util.PlaySound("sfx_ducttape_active", base.gameObject);
                    }
                }
                else if (body.HasBuff(ScrapyardContent.Buffs.bdDuctTape))
                {
                    if (NetworkServer.active) body.RemoveBuff(ScrapyardContent.Buffs.bdDuctTape);
                }
            }
        }
    }
}
