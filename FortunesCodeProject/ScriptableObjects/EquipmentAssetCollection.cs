using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "EquipmentAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/EquipmentAssetCollection")]
    public class EquipmentAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public EquipmentDef equipmentDef;
    }
}