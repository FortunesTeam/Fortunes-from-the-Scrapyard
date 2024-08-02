﻿using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Equipments
{
    public sealed class EnergyBar : ScrapyardEquipment
    {
        public const string TOKEN = "SCRAPYARD_EQUIP_ENERGYBAR_DESC";

        //[ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        //public static float regenAmount = 15f;
        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float speedBonus = 0.90f;
        [ConfigureField(ScrapyardConfig.ID_EQUIPS)]
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
                    body.AddTimedBuff(ScrapyardContent.Buffs.bdEnergyBar, buffDuration);
                    body.AddTimedBuff(RoR2Content.Buffs.CrocoRegen, buffDuration);
                }
                body.healthComponent.ForceShieldRegen();
                Util.PlaySound("sfx_energybar_use", body.gameObject);
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += EnergyBarScrapyardContent.Buffs.bdFaultyTurbo;
        }

        private void EnergyBarScrapyardContent.Buffs.bdFaultyTurbo(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(ScrapyardContent.Buffs.bdEnergyBar);
            args.moveSpeedMultAdd += speedBonus * buffCount;
            //args.regenMultAdd += regenAmount * buffCount;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<EquipmentAssetCollection>("acEnergyBar", ScrapyardBundle.Equipments);
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