using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FortunesFromTheScrapyard.Modules.HitHooks;
using FortunesFromTheScrapyard.Modules;
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Items
{
    public class SprayCan : ItemBase<SprayCan>
    {
        [AutoConfig("Poison Proc Chance Base", 11)]
        public static int baseChance = 11;
        [AutoConfig("Poison Proc Chance Stacking", 7)]
        public static int stackChance = 7;
        [AutoConfig("Poison Damage Coefficient Total", 2)]
        public static float poisonTotalDamage = 2;
        [AutoConfig("Poison Damage Coefficient Tick", 0.1f)]
        public static float poisonTickDamage = 0.1f;
        [AutoConfig("Poison Tick Rate", 0.1f)]
        public static float poisonTickRate = 0.1f;
        public static DotController.DotDef poisonDotDef;
        public static DotController.DotIndex poisonDotIndex;
        public override void Init()
        {
            itemName = "SprayCan";
            base.Init();
        }
        public override void Hooks()
        {
            GetHitBehavior += SprayCanHit;

            poisonDotDef = new DotController.DotDef
            {
                associatedBuff = FortunesContent.contentPack.buffDefs.Find("SprayCanDebuff"),
                damageCoefficient = poisonTickDamage,
                damageColorIndex = DamageColorIndex.Poison,
                interval = poisonTickRate
            };
            poisonDotIndex = DotAPI.RegisterDotDef(poisonDotDef, (self, dotStack) =>
            {

            });
        }

        private void SprayCanHit(CharacterBody attackerBody, DamageInfo damageInfo, CharacterBody victimBody)
        {
            int sprayCount = GetCount(attackerBody);
            if(sprayCount > 0)
            {
                float chance = GetStackValue(baseChance, stackChance, sprayCount);
                float finalChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(chance);
                if(Util.CheckRoll(finalChance, attackerBody.master))
                {
                    uint? maxStacksFromAttacker = null;
                    if ((damageInfo != null) ? damageInfo.inflictor : null)
                    {
                        ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                        if (component && component.useDotMaxStacksFromAttacker)
                        {
                            maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                        }
                    }

                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                    {
                        attackerObject = damageInfo.attacker,
                        victimObject = victimBody.gameObject,
                        totalDamage = new float?(damageInfo.damage * poisonTotalDamage),
                        damageMultiplier = 1f,
                        dotIndex = poisonDotIndex,
                        maxStacksFromAttacker = maxStacksFromAttacker
                    };
                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }
    }
}
