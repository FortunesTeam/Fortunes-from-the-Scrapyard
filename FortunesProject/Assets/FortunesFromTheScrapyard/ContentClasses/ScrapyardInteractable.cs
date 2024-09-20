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
    /// <inheritdoc cref="IInteractableContentPiece"/>
    /// </summary>
    public abstract class ScrapyardInteractable : IInteractableContentPiece, IContentPackModifier
    {
        public InteractableAssetCollection assetCollection { get; private set; }
        public InteractableCardProvider cardProvider { get; protected set; }
        IInteractable IGameObjectContentPiece<IInteractable>.component => interactablePrefab.GetComponent<IInteractable>();
        GameObject IContentPiece<GameObject>.asset => interactablePrefab;
        public GameObject interactablePrefab { get; protected set; }


        NullableRef<InteractableCardProvider> IInteractableContentPiece.cardProvider => cardProvider;

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest<InteractableAssetCollection> LoadAssetRequest();
        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<InteractableAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            assetCollection = request.asset;

            cardProvider = assetCollection.interactableCardProvider;
            interactablePrefab = assetCollection.interactablePrefab;

        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }
    }
}