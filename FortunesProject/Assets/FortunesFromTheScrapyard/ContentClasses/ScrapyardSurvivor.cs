using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;
namespace FortunesFromTheScrapyard
{
    /// <summary>
    /// <inheritdoc cref="ISurvivorContentPiece"/>
    /// </summary>
    public abstract class ScrapyardSurvivor : ISurvivorContentPiece, IContentPackModifier
    {
        public  SurvivorAssetCollection AssetCollection { get; private set; }
        public SurvivorDef SurvivorDef { get; protected  set; }
        public NullableRef<GameObject> MasterPrefab { get; protected set; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.Component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.Asset => CharacterPrefab;
        public GameObject CharacterPrefab { get; protected set; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest();

        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<SurvivorAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            AssetCollection = request.asset;

            CharacterPrefab = AssetCollection.bodyPrefab;
            MasterPrefab = AssetCollection.masterPrefab;
            SurvivorDef = AssetCollection.survivorDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}