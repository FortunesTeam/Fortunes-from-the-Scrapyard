using MSU;
using R2API;
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
    /// <inheritdoc cref="IMonsterContentPiece"/>
    /// </summary>
    public abstract class ScrapyardMonster : IMonsterContentPiece, IContentPackModifier
    {
        public NullableRef<MonsterCardProvider> cardProvider { get; protected set; }
        public NullableRef<DirectorCardHolderExtended> dissonanceCard { get; protected set; }
        public MonsterAssetCollection assetCollection { get; private set; }
        public NullableRef<GameObject> masterPrefab { get; protected set; }

        NullableRef<DirectorCardHolderExtended> IMonsterContentPiece.dissonanceCard => dissonanceCard;
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => characterPrefab.GetComponent<CharacterBody>();
        NullableRef<MonsterCardProvider> IMonsterContentPiece.cardProvider => cardProvider;
        GameObject IContentPiece<GameObject>.asset => characterPrefab;
        public GameObject characterPrefab { get; private set; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest<MonsterAssetCollection> LoadAssetRequest();
        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<MonsterAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            assetCollection = request.asset;

            characterPrefab = assetCollection.bodyPrefab;
            masterPrefab = assetCollection.masterPrefab;
            cardProvider = assetCollection.monsterCardProvider;
        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }
    }
}
