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
        public  SurvivorAssetCollection assetCollection { get; private set; }
        public SurvivorDef survivorDef { get; protected  set; }
        public NullableRef<GameObject> masterPrefab { get; protected set; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => characterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.asset => characterPrefab;
        public GameObject characterPrefab { get; protected set; }

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

            assetCollection = request.asset;

            characterPrefab = assetCollection.bodyPrefab;
            masterPrefab = assetCollection.masterPrefab;
            survivorDef = assetCollection.survivorDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }
    }
}