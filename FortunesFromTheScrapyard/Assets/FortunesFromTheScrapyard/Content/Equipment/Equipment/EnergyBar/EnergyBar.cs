using BepInEx.Configuration;
using FortunesFromTheScrapyard.Modules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Equipment
{
    public class EnergyBar : EquipmentBase<EnergyBar>
    {
        [AutoConfig("Cooldown", 12f)]
        public static float cooldown = 12f;
        [AutoConfig("Speed Bonus", 0.45f)]
        public static float speedBonus = 0.45f;
        [AutoConfig("Speed Bonus Duration", 5f)]
        public static float speedBonusDuration = 5f;

        public override void Init()
        {
            equipName = "EnergyBar";
            base.Init();
        }

        public override void Hooks()
        {
            GetStatCoefficients += EnergyBarSpeedBuff;
        }

        private void EnergyBarSpeedBuff(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(FortunesContent.contentPack.buffDefs.Find("EnergyBarBuffDef"));
            args.moveSpeedMultAdd += speedBonus * buffCount;
        }
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            CharacterBody body = slot.characterBody;
            SkillLocator skill = body.skillLocator;
            if(skill != null)
            {
                skill.ApplyAmmoPack();
                body.AddTimedBuffAuthority(FortunesContent.contentPack.buffDefs.Find("EnergyBarBuffDef").buffIndex, speedBonusDuration);
                return true;
            }
            return false;
        }
    }
}
