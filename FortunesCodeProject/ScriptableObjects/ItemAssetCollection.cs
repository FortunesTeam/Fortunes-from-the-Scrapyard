using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "ItemAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/ItemAssetCollection")]
    public class ItemAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public ItemDef itemDef;
    }
}