using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Equipments
{
    public sealed class EnergyBar : FFTSEquipment
    {
        public const string TOKEN = "FFTS_EQUIP_ENERGYBAR_DESCRIPTION";

        //[ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        //public static float regenAmount = 15f;
        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float speedBonus = 0.90f;
        [ConfigureField(FFTSConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, 1)]
        public static float buffDuration = 2f;
        public override bool Execute(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            SkillLocator skill = body.skillLocator;
            if (skill != null)
            {
                if (NetworkServer.active)
                {
                    body.AddTimedBuff(FFTSContent.Buffs.bdEnergyBar, buffDuration);
                    body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, buffDuration);
                }
                body.healthComponent.ForceShieldRegen();
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += EnergyBarStats;
        }

        private void EnergyBarStats(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(FFTSContent.Buffs.bdEnergyBar);
            args.moveSpeedMultAdd += speedBonus * buffCount;
            //args.regenMultAdd += regenAmount * buffCount;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<EquipmentAssetCollection>("acEnergyBar", FFTSBundle.Equipments);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}