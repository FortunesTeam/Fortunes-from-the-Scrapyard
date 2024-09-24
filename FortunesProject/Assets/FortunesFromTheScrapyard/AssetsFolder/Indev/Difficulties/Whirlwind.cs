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
    public class Whirlwind : ScrapyardDifficulty
    {
        public override ScrapyardAssetRequest<SerializableDifficultyDef> AssetRequest => ScrapyardAssets.LoadAssetAsync<SerializableDifficultyDef>("Whirlwind", ScrapyardBundle.Indev);

        public static SerializableDifficultyDef whirlwindDifficulty;

        [ConfigureField(ScrapyardConfig.ID_DIFFICULTY)]
        internal static float attackSpeed = 1.5f;
        [ConfigureField(ScrapyardConfig.ID_DIFFICULTY)]
        internal static float moveSpeed = 1.3f;
        [ConfigureField(ScrapyardConfig.ID_DIFFICULTY)]
        internal static float cdr = 0.5f;
        [ConfigureField(ScrapyardConfig.ID_DIFFICULTY)]
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

                PredictionHooks.UnInit();

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

                PredictionHooks.Init();

                AllowPostLoopElites(true);
            }

        }
        private static void AllowPostLoopElites(bool enable)
        {
            CombatDirector.EliteTierDef[] eliteTiers = CombatDirector.eliteTiers;
            foreach (CombatDirector.EliteTierDef eliteTierDef in eliteTiers)
            {
                if (enable && !eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Poison) && !eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Haunted) && 
                    !eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Lunar) && !eliteTierDef.eliteTypes.Contains(DLC2Content.Elites.Aurelionite) && !eliteTierDef.eliteTypes.Contains(DLC2Content.Elites.Bead) && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Fire))
                {
                    Array.Resize(ref eliteTierDef.eliteTypes, eliteTierDef.eliteTypes.Length + 1);
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)] = RoR2Content.Elites.Poison;

                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].damageBoostCoefficient /= 2f;
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].healthBoostCoefficient /= 8f;

                    Array.Resize(ref eliteTierDef.eliteTypes, eliteTierDef.eliteTypes.Length + 1);
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)] = RoR2Content.Elites.Haunted;

                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].damageBoostCoefficient /= 2f;
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].healthBoostCoefficient /= 8f;

                    Array.Resize(ref eliteTierDef.eliteTypes, eliteTierDef.eliteTypes.Length + 1);
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)] = DLC2Content.Elites.Aurelionite;

                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].damageBoostCoefficient /= 2f;
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].healthBoostCoefficient /= 8f;

                    Array.Resize(ref eliteTierDef.eliteTypes, eliteTierDef.eliteTypes.Length + 1);
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)] = DLC2Content.Elites.Bead;

                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].damageBoostCoefficient /= 2f;
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)].healthBoostCoefficient /= 8f;

                    Array.Resize(ref eliteTierDef.eliteTypes, eliteTierDef.eliteTypes.Length + 1);
                    eliteTierDef.eliteTypes[eliteTierDef.eliteTypes.GetUpperBound(0)] = RoR2Content.Elites.Lunar;

                    break;
                }
                else if (!enable && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Poison) && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Haunted) && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Lunar) && eliteTierDef.eliteTypes.Contains(RoR2Content.Elites.Fire))
                {
                    List<EliteDef> list = new List<EliteDef>(eliteTierDef.eliteTypes);

                    eliteTierDef.costMultiplier = 6f;

                    for (int i = eliteTierDef.eliteTypes.Length - 1; i >= 0; i--)
                    {
                        if (eliteTierDef.eliteTypes[i])
                        {
                            if (eliteTierDef.eliteTypes[i].eliteIndex == RoR2Content.Elites.Poison.eliteIndex || eliteTierDef.eliteTypes[i].eliteIndex == DLC2Content.Elites.Bead.eliteIndex || 
                                eliteTierDef.eliteTypes[i].eliteIndex == RoR2Content.Elites.Haunted.eliteIndex || eliteTierDef.eliteTypes[i].eliteIndex == DLC2Content.Elites.Aurelionite.eliteIndex)
                            {
                                eliteTierDef.eliteTypes[i].damageBoostCoefficient = 6f;
                                eliteTierDef.eliteTypes[i].healthBoostCoefficient = 18f;
                                list.RemoveAt(i);
                            }
                            else if (eliteTierDef.eliteTypes[i].eliteIndex == RoR2Content.Elites.Lunar.eliteIndex)
                            {
                                list.RemoveAt(i);
                            }
                        }
                    }
                    eliteTierDef.eliteTypes = list.ToArray();

                    break;
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
                self.attackSpeed *= attackSpeed;

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
            self.goldRewardCoefficient *= 0.85f;
            orig(self);
        }
    }
}