using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "EliteAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/EliteAssetCollection")]
    public class EliteAssetCollection : EquipmentAssetCollection
    {
        public List<EliteDef> eliteDefs;
    }
}