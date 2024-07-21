﻿using MSU;
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
    public abstract class ScrapyardVoidItem : IVoidItemContentPiece, IContentPackModifier
    {
        public ItemAssetCollection AssetCollection { get; private set; }
        public NullableRef<List<GameObject>> ItemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        public ItemDef ItemDef { get; protected set; }

        ItemDef IContentPiece<ItemDef>.Asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.ItemDisplayPrefabs => ItemDisplayPrefabs;

        public abstract ScrapyardAssetRequest LoadAssetRequest();


        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            if (request.boxedAsset is ItemAssetCollection collection)
            {
                AssetCollection = collection;

                ItemDef = AssetCollection.itemDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;
            }
            else if (request.boxedAsset is ItemDef def)
            {
                ItemDef = def;
            }
            else
            {
                ScrapyardLog.Error("Invalid AssetRequest " + request.assetName + " of type " + request.boxedAsset.GetType());
            }
        }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public abstract List<ItemDef> GetInfectableItems();
    }
}