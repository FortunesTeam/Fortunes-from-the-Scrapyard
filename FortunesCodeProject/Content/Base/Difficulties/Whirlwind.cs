using MSU.Config;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;

namespace FortunesFromTheScrapyard
{
    public class Whirlwind : FFTSDifficulty
    {
        public override FFTSAssetRequest<SerializableDifficultyDef> AssetRequest => FFTSAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", FFTSBundle.Difficulties);

        public static SerializableDifficultyDef whirlwindDifficulty;

        private static readonly Dictionary<CombatDirector.EliteTierDef, (EliteDef[] original, EliteDef[] expanded)> modifiedEliteTiers = new Dictionary<CombatDirector.EliteTierDef, (EliteDef[] original, EliteDef[] expanded)>();
        private static readonly Dictionary<EliteDef, EliteDef> whirlwindEliteDefs = new Dictionary<EliteDef, EliteDef>();
        private static readonly HashSet<CombatDirector.EliteTierDef> postLoopEliteTiers = new HashSet<CombatDirector.EliteTierDef>();

        [FFTSConfigureField(FFTSConfig.ID_DIFFICULTY, 0f, 5f)]
        internal static float moveSpeed = 1.3f;
        [FFTSConfigureField(FFTSConfig.ID_DIFFICULTY, 0f, 2f)]
        internal static float cdr = 0.25f;
        [FFTSConfigureField(FFTSConfig.ID_DIFFICULTY, -100f, 100f)]
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

                On.RoR2.CombatDirector.EliteTierDef.CanSelect -= EliteTierDef_CanSelect;

                RestoreEliteTiers();
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

                AllowPostLoopElites();

                On.RoR2.CombatDirector.EliteTierDef.CanSelect += EliteTierDef_CanSelect;
            }

        }
        private static void AllowPostLoopElites()
        {
            if (modifiedEliteTiers.Count > 0)
                return;

            CombatDirector.EliteTierDef[] eliteTiers = CombatDirector.eliteTiers
                .Where(tier => tier.eliteTypes != null && tier.eliteTypes.Contains(RoR2Content.Elites.Fire)).ToArray();
            if (eliteTiers.Length == 0)
            {
                FFTSLog.Warning("Whirlwind could not find a normal elite tier to extend.");
                return;
            }

            CombatDirector.EliteTierDef[] postLoopTiers = CombatDirector.eliteTiers
                .Where(tier => tier.eliteTypes != null
                    && tier.eliteTypes.Contains(RoR2Content.Elites.Poison)
                    && tier.eliteTypes.Contains(RoR2Content.Elites.Haunted)
                    && !eliteTiers.Contains(tier)).ToArray();
            if (postLoopTiers.Length == 0)
            {
                FFTSLog.Warning("Whirlwind could not find the post-loop elite tier to extend.");
                return;
            }

            EliteDef collective = EliteCatalog.eliteList.Select(EliteCatalog.GetEliteDef)
                .FirstOrDefault(elite => elite && elite.name == "edCollective");
            if (!collective)
                FFTSLog.Warning("Whirlwind could not find Collective in the elite catalog; it will not be added to the elite pool.");

            EliteDef[] additionalElites = postLoopTiers.SelectMany(tier => tier.eliteTypes)
                .Concat(new[] { DLC2Content.Elites.Aurelionite, RoR2Content.Elites.Lunar, collective })
                .Where(elite => elite).Distinct().ToArray();

            foreach (EliteDef elite in additionalElites)
            {
                if (elite == DLC2Content.Elites.Aurelionite || elite == RoR2Content.Elites.Lunar)
                    continue;

                EliteDef whirlwindElite = Object.Instantiate(elite);
                whirlwindElite.name = elite.name;
                whirlwindElite.eliteIndex = elite.eliteIndex;
                whirlwindElite.damageBoostCoefficient = RoR2Content.Elites.Fire.damageBoostCoefficient;
                whirlwindElite.healthBoostCoefficient = RoR2Content.Elites.Fire.healthBoostCoefficient;
                whirlwindEliteDefs.Add(elite, whirlwindElite);
            }

            foreach (CombatDirector.EliteTierDef eliteTier in eliteTiers)
            {
                EliteDef[] originalElites = eliteTier.eliteTypes;
                EliteDef[] expandedElites = originalElites.Concat(additionalElites).Distinct()
                    .Select(elite => elite && whirlwindEliteDefs.TryGetValue(elite, out EliteDef replacement) ? replacement : elite)
                    .ToArray();
                modifiedEliteTiers.Add(eliteTier, (originalElites, expandedElites));
                eliteTier.eliteTypes = expandedElites;
            }

            postLoopEliteTiers.UnionWith(postLoopTiers);
            FFTSLog.Info($"Whirlwind expanded {eliteTiers.Length} normal elite tiers with {string.Join(", ", additionalElites.Select(elite => elite.name))}. Tier cost multipliers: {string.Join(", ", eliteTiers.Select(tier => tier.costMultiplier).Distinct())}.");
        }

        private static bool EliteTierDef_CanSelect(On.RoR2.CombatDirector.EliteTierDef.orig_CanSelect orig, CombatDirector.EliteTierDef self, SpawnCard.EliteRules rules)
        {
            return !postLoopEliteTiers.Contains(self) && orig(self, rules);
        }

        private static void RestoreEliteTiers()
        {
            Dictionary<EliteDef, EliteDef> originalDefs = whirlwindEliteDefs.ToDictionary(entry => entry.Value, entry => entry.Key);
            foreach (var entry in modifiedEliteTiers)
            {
                if (entry.Key.eliteTypes == entry.Value.expanded)
                {
                    entry.Key.eliteTypes = entry.Value.original;
                    continue;
                }

                HashSet<EliteDef> addedElites = new HashSet<EliteDef>(entry.Value.expanded);
                addedElites.ExceptWith(entry.Value.original);
                foreach (EliteDef elite in entry.Value.original)
                    if (elite && whirlwindEliteDefs.TryGetValue(elite, out EliteDef replacement))
                        addedElites.Remove(replacement);

                entry.Key.eliteTypes = entry.Key.eliteTypes.Where(elite => !addedElites.Contains(elite))
                    .Select(elite => elite && originalDefs.TryGetValue(elite, out EliteDef original) ? original : elite)
                    .Distinct().ToArray();
            }
            modifiedEliteTiers.Clear();
            postLoopEliteTiers.Clear();

            foreach (EliteDef elite in whirlwindEliteDefs.Values)
                Object.Destroy(elite);
            whirlwindEliteDefs.Clear();
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