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
using BepInEx;
using System.Collections.Generic;

namespace FortunesFromTheScrapyard
{
    public class Whirlwind : FFTSDifficulty
    {
        public override FFTSAssetRequest<SerializableDifficultyDef> AssetRequest => FFTSAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", FFTSBundle.Difficulties);

        public static SerializableDifficultyDef whirlwindDifficulty;

        private static readonly Dictionary<CombatDirector.EliteTierDef, List<EliteDef>> addedEliteTypes = new Dictionary<CombatDirector.EliteTierDef, List<EliteDef>>();
        private static readonly Dictionary<EliteDef, (float damage, float health)> originalEliteCoefficients = new Dictionary<EliteDef, (float damage, float health)>();

        [ConfigureField(FFTSConfig.ID_DIFFICULTY)]
        internal static float moveSpeed = 1.3f;
        [ConfigureField(FFTSConfig.ID_DIFFICULTY)]
        internal static float cdr = 0.25f;
        [ConfigureField(FFTSConfig.ID_DIFFICULTY)]
        internal static float teleporterRadius = -30f;
        public override void Initialize()
        {
            whirlwindDifficulty = difficultyDef;
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnRunEnd(Run run)
        {
            if (DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty) == whirlwindDifficulty.DifficultyDef)
            {
                On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;

                On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;

                On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;

                AllowPostLoopElites(false);
            }
        }

        public override void OnRunStart(Run run)
        {
            if (DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty) == whirlwindDifficulty.DifficultyDef)
            {
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;

                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

                On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;

                foreach (CharacterMaster cm in run.userMasters.Values)
                    if (NetworkServer.active)
                        cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);

                AllowPostLoopElites(true);
            }

        }
        private static void AllowPostLoopElites(bool enable)
        {
            if (!enable)
            {
                foreach (var entry in originalEliteCoefficients)
                {
                    entry.Key.damageBoostCoefficient = entry.Value.damage;
                    entry.Key.healthBoostCoefficient = entry.Value.health;
                }
                originalEliteCoefficients.Clear();

                foreach (var entry in addedEliteTypes)
                {
                    entry.Key.eliteTypes = entry.Key.eliteTypes.Where(elite => !entry.Value.Contains(elite)).ToArray();
                }
                addedEliteTypes.Clear();
                return;
            }

            if (addedEliteTypes.Count > 0)
                return;

            CombatDirector.EliteTierDef[] eliteTiers = CombatDirector.eliteTiers
                .Where(tier => tier.eliteTypes.Contains(RoR2Content.Elites.Fire)).ToArray();
            if (eliteTiers.Length == 0)
            {
                FFTSLog.Warning("Whirlwind could not find a normal elite tier to extend.");
                return;
            }

            List<EliteDef> postLoopElites = new List<EliteDef>
            {
                RoR2Content.Elites.Poison,
                RoR2Content.Elites.Haunted,
                DLC2Content.Elites.Aurelionite,
                DLC2Content.Elites.Bead,
                RoR2Content.Elites.Lunar
            };

            // Resolve DLC3 through the catalog because the build references pre-DLC3 game libraries.
            EliteDef collective = EliteCatalog.eliteList.Select(EliteCatalog.GetEliteDef)
                .FirstOrDefault(elite => elite && elite.name == "edCollective");
            if (collective)
                postLoopElites.Add(collective);
            else
                FFTSLog.Warning("Whirlwind could not find DLC3's Collective elite in the elite catalog.");

            foreach (CombatDirector.EliteTierDef eliteTierDef in eliteTiers)
            {
                List<EliteDef> addedElites = postLoopElites.Where(elite => !eliteTierDef.eliteTypes.Contains(elite)).ToList();
                if (addedElites.Count == 0)
                    continue;

                addedEliteTypes.Add(eliteTierDef, addedElites);
                eliteTierDef.eliteTypes = eliteTierDef.eliteTypes.Concat(addedElites).ToArray();

                foreach (EliteDef elite in addedElites)
                {
                    // EliteDefs are shared by tiers, so reduce each elite only once per run.
                    if (elite == RoR2Content.Elites.Lunar || originalEliteCoefficients.ContainsKey(elite))
                        continue;

                    originalEliteCoefficients.Add(elite, (elite.damageBoostCoefficient, elite.healthBoostCoefficient));
                    elite.damageBoostCoefficient /= 2f;
                    elite.healthBoostCoefficient /= 8f;
                }
            }
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
                if (self.bodyIndex != BodyCatalog.FindBodyIndex("BrotherBody"))
                {
                    self.moveSpeed *= moveSpeed;
                }

                if (self.skillLocator)
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
            self.goldRewardCoefficient *= 0.75f;
            orig(self);
        }
    }
}