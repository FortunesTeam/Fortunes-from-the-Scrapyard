using RoR2;
using UnityEngine;
using MSU;
using System.Collections.Generic;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "MonsterAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/MonsterAssetCollection")]
    public class MonsterAssetCollection : BodyAssetCollection
    {
        public MonsterCardProvider monsterCardProvider;
        public DirectorCardHolderExtended dissonanceCardHolder;
    }
}