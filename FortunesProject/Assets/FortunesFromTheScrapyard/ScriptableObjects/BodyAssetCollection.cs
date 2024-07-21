using RoR2;
using UnityEngine;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "BodyAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/BodyAssetCollection")]
    public class BodyAssetCollection : ExtendedAssetCollection
    {
        public GameObject bodyPrefab;
        public GameObject masterPrefab;
    }
}
