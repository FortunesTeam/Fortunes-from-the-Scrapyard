using RoR2;
using UnityEngine;
using System.Collections.Generic;
using RoR2.Artifacts;
using R2API.ScriptableObjects;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "ArtifactAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/ArtifactAssetCollection")]
    public class ArtifactAssetCollection : ExtendedAssetCollection
    {
        public ArtifactCode artifactCode;
        public ArtifactDef artifactDef;
    }
}