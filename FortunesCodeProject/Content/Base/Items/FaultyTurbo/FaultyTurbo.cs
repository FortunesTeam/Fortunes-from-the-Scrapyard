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
    public class FaultyTurbo : FFTSItem
    {
        public const string TOKEN = "FFTS_ITEM_FAULTYTURBO_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        public static int maxStacks = 3;
        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static int maxStacksPerStack = 1;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float movespeedBonusPerInterval = 0.1f; //0.1f

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float movespeedBonusPerIntervalStack = 0.025f; //0.1f

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float checkInterval = 1f;

        internal static NetworkSoundEventDef turboSfx;
        public override void Initialize()
        {
            turboSfx = FFTSContent.CreateAndAddNetworkSoundEventDef("sfx_turbo_start");
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acFaultyTurbo", FFTSBundle.Items);
        }

        public class FaultyTurboBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => FFTSContent.Items.FaultyTurbo;

            private float timer = 0f;
            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(FFTSContent.Buffs.bdFaultyTurbo))
                {
                    args.moveSpeedMultAdd += GetStackValue(movespeedBonusPerInterval, movespeedBonusPerIntervalStack, stack) * body.GetBuffCount(FFTSContent.Buffs.bdFaultyTurbo);
                }
            }

            private void FixedUpdate()
            {
                bool atMaxStacks = body.GetBuffCount(FFTSContent.Buffs.bdFaultyTurbo) >= GetStackValue(maxStacks, maxStacksPerStack, stack);

                if (base.body.isSprinting && !atMaxStacks)
                {
                    timer += Time.fixedDeltaTime;
                }
                else if (body.HasBuff(FFTSContent.Buffs.bdFaultyTurbo)) timer -= Time.fixedDeltaTime;

                if (timer >= checkInterval && !atMaxStacks && base.body.isSprinting)
                {
                    timer = 0f;

                    if (NetworkServer.active)
                    {
                        body.AddBuff(FFTSContent.Buffs.bdFaultyTurbo);
                    }

                    EffectManager.SimpleSoundEffect(turboSfx.index, body.corePosition, false);
                }
                else if (timer < 0f && body.HasBuff(FFTSContent.Buffs.bdFaultyTurbo) && !base.body.isSprinting)
                {
                    timer = checkInterval / 2f;
                    body.RemoveBuff(FFTSContent.Buffs.bdFaultyTurbo);
                }
            }
            private void OnDisable()
            {
                if (NetworkServer.active)
                {
                    if (body.HasBuff(FFTSContent.Buffs.bdFaultyTurbo))
                    {
                        body.SetBuffCount(FFTSContent.Buffs.bdFaultyTurbo.buffIndex, 0);
                    }
                }
            }
        }
    }
}
