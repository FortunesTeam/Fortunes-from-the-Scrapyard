﻿using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors
{
    public sealed class Predator : ScrapyardSurvivor
    {
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acPredator", ScrapyardBundle.Indev);
        }
    }
}

