using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortunesFromTheScrapyard
{
    public sealed class Gump : ScrapyardMonster
    {
        public override ScrapyardAssetRequest<MonsterAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<MonsterAssetCollection>("acGump", ScrapyardBundle.Indev);
        }

        public static GameObject _masterPrefab;

        public override void Initialize()
        {
            
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
