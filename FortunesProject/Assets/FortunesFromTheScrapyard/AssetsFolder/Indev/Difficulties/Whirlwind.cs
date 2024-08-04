using MSU.Config;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using RoR2.CharacterAI;
using RoR2.Projectile;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using HG;

namespace FortunesFromTheScrapyard
{
    public class Whirlwind : ScrapyardDifficulty
    {
        public override ScrapyardAssetRequest<SerializableDifficultyDef> AssetRequest => ScrapyardAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", ScrapyardBundle.Indev);

        public static SerializableDifficultyDef whirlwindDifficulty;

        internal static bool prediction = true;
        internal static float attackSpeed = 1.5f;
        internal static float moveSpeed = 1.5f;
        internal static float cdr = 0.5f;
        internal static float teleporterRadius = -50f;
        public override void Initialize()
        {
            whirlwindDifficulty = DifficultyDef;
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnRunEnd(Run run)
        {
            On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;

            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;

            On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;
        }

        public override void OnRunStart(Run run)
        {
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;

            foreach (CharacterMaster cm in run.userMasters.Values)
                if (NetworkServer.active)
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
        }

        private void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig.Invoke(self);
            self.calcRadius += Self_calcRadius;
        }
        public static void Self_calcRadius(ref float radius)
        {
            radius *= Mathf.Max(1f + teleporterRadius / 100f, 0f);
        }
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.teamComponent.teamIndex == TeamIndex.Monster)
            {
                if (self.baseNameToken != "BROTHER_BODY_NAME")
                {
                    self.moveSpeed *= moveSpeed;
                }
                self.attackSpeed *= attackSpeed;

                if(self.skillLocator)
                {
                    if (self.skillLocator.primary) self.skillLocator.primary.cooldownScale *= cdr;
                    if (self.skillLocator.secondary) self.skillLocator.secondary.cooldownScale *= cdr;
                    if (self.skillLocator.utility) self.skillLocator.utility.cooldownScale *= cdr;
                    if (self.skillLocator.special) self.skillLocator.special.cooldownScale *= cdr;
                }
            }
        }
        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            self.creditMultiplier *= 1.5f;
            self.expRewardCoefficient *= 0.75f;
            self.goldRewardCoefficient *= 0.75f;
            orig(self);
        }
    }
}


