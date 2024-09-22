using MSU;
using R2API.ScriptableObjects;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using static RoR2.CombatDirector;
using System.Linq;

namespace FortunesFromTheScrapyard
{
    public class WhirlwindEliteTierDef : ScrapyardEliteTier
    {
        public override ScrapyardAssetRequest<SerializableEliteTierDef> AssetRequest => ScrapyardAssets.LoadAssetAsync<SerializableEliteTierDef>("WhirlwindSpecialEliteTierDef", ScrapyardBundle.Indev);

        public override void Initialize()
        {
            isAvailableCheck = (SpawnCard.EliteRules rules) => DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty) == Whirlwind.whirlwindDifficulty.DifficultyDef;
            
            foreach(EliteIndex eliteIndex in EliteCatalog.eliteList)
            {
                EliteDef elite = EliteCatalog.GetEliteDef(eliteIndex);
                if (elite.healthBoostCoefficient <= 8f || elite.damageBoostCoefficient <= 5f && !eliteTierDef.elites.ToList().Contains(elite))
                {
                    eliteTierDef.elites.ToList().Add(elite);
                }
            }

            eliteTierDef.elites.ToArray();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}