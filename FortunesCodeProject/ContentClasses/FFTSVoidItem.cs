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
    /// <inheritdoc cref="IVoidItemContentPiece"/>
    /// </summary>
    public abstract class FFTSVoidItem : IVoidItemContentPiece, IContentPackModifier
    {
        public ItemAssetCollection assetCollection { get; private set; }
        public NullableRef<List<GameObject>> itemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        public ItemDef ItemDef { get; protected set; }

        ItemDef IContentPiece<ItemDef>.asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.itemDisplayPrefabs => itemDisplayPrefabs;

        public abstract FFTSAssetRequest LoadAssetRequest();


        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            FFTSAssetRequest request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            if (request.boxedAsset is ItemAssetCollection collection)
            {
                assetCollection = collection;

                ItemDef = assetCollection.itemDef;
                itemDisplayPrefabs = assetCollection.itemDisplayPrefabs;
            }
            else if (request.boxedAsset is ItemDef def)
            {
                ItemDef = def;
            }
            else
            {
                FFTSLog.Error("Invalid AssetRequest " + request.assetName + " of type " + request.boxedAsset.GetType());
            }
        }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public abstract List<ItemDef> GetInfectableItems();
    }
}