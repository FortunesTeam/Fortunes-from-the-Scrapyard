using MSU;
using RoR2;
using UnityEngine;
namespace FortunesFromTheScrapyard
{
    [CreateAssetMenu(fileName = "SceneAssetCollection", menuName = "FortunesFromTheScrapyard/AssetCollections/SceneAssetCollection")]
    public class SceneAssetCollection : ExtendedAssetCollection
    {
        public SceneDef sceneDef;
        public NullableRef<MusicTrackDef> mainTrack;
        public NullableRef<MusicTrackDef> bossTrack;

        [Header("Stage Registration Metadata")]
        public float stageWeightRelativeToSiblings;
        public bool appearsPreLoop;
        public bool appearsPostLoop;
    }
}
