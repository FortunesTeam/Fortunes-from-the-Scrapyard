using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MSU;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "InteractableAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/InteractableAssetCollection")]
    public class InteractableAssetCollection : ExtendedAssetCollection
    {
        public GameObject interactablePrefab;
        public InteractableCardProvider interactableCardProvider;
    }
}