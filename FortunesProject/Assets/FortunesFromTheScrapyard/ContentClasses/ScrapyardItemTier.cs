using MSU;
using R2API.ScriptableObjects;
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
    /// <inheritdoc cref="IItemTierContentPiece"/>
    /// </summary>
    public abstract class ScrapyardItemTier : IItemTierContentPiece, IContentPackModifier
    {
        public ItemTierAssetCollection assetCollection { get; private set; }
        public NullableRef<SerializableColorCatalogEntry> colorIndex { get; protected set; }
        public NullableRef<SerializableColorCatalogEntry> darkColorIndex { get; protected set; }
        public GameObject pickupDisplayVFX { get; protected set; }
        public List<ItemIndex> itemsWithThisTier { get; set; } = new List<ItemIndex>();
        public List<PickupIndex> availableTierDropList { get; set; } = new List<PickupIndex>();
        ItemTierDef IContentPiece<ItemTierDef>.asset => itemTierDef;
        public ItemTierDef itemTierDef { get; protected set;  }

        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest<ItemTierAssetCollection> LoadAssetRequest();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<ItemTierAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            assetCollection = request.asset;
            itemTierDef = assetCollection.itemTierDef;
            
            if (assetCollection.colorIndex)
                colorIndex = assetCollection.colorIndex;
            if (assetCollection.darkColorIndex)
                darkColorIndex = assetCollection.darkColorIndex;

            pickupDisplayVFX = assetCollection.pickupDisplayVFX;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }
    }
}