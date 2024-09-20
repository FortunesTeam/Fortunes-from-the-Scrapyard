using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace FortunesFromTheScrapyard
{
    public abstract class ScrapyardScene : ISceneContentPiece, IContentPackModifier
    {
        public SceneAssetCollection assetCollection { get; private set; }
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public NullableRef<MusicTrackDef> mainTrack { get; protected set; }
        public NullableRef<MusicTrackDef> bossTrack { get; protected set; }

        public NullableRef<Texture2D> bazaarTextureBase { get; protected set; } // ???

        public SceneDef asset { get; protected set; }

        float? ISceneContentPiece.weightRelativeToSiblings => assetCollection.weightRelativeToSiblings;

        bool? ISceneContentPiece.preLoop => assetCollection.appearsPreLoop;

        bool? ISceneContentPiece.postLoop => assetCollection.appearsPostLoop;

        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest<SceneAssetCollection> LoadAssetRequest();

        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<SceneAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            assetCollection = request.asset;

            asset = assetCollection.sceneDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public virtual void OnServerStageComplete(Stage stage)
        {
        }

        public virtual void OnServerStageBegin(Stage stage)
        {           
        }
    }
}
