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
    /// <inheritdoc cref="IEquipmentContentPiece"/>
    /// </summary>
    public abstract class ScrapyardEquipment : IEquipmentContentPiece, IContentPackModifier
    {
        /// <summary>
        /// The EquipmentAssetCollection for this Artifact. Populated when the Artifact gets it's assets loaded, can be null.
        /// </summary>
        public NullableRef<EquipmentAssetCollection> assetCollection { get; private set; }

        /// <summary>
        /// <inheritdoc cref="IEquipmentContentPiece.ItemDisplayPrefabs"/>
        /// </summary>
        public NullableRef<List<GameObject>> itemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        /// <summary>
        /// <inheritdoc cref="IContentPiece{T}.Asset"/>
        /// </summary>
        public EquipmentDef equipmentDef { get; protected set; }
        
        NullableRef<List<GameObject>> IEquipmentContentPiece.ItemDisplayPrefabs => itemDisplayPrefabs;
        EquipmentDef IContentPiece<EquipmentDef>.Asset => equipmentDef;


        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest</returns>
        public abstract ScrapyardAssetRequest LoadAssetRequest();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            if (request.boxedAsset is EquipmentAssetCollection collection)
            {
                assetCollection = collection;

                equipmentDef = collection.equipmentDef;
                itemDisplayPrefabs = collection.itemDisplayPrefabs;
            }
            else if (request.boxedAsset is EquipmentDef def)
            {
                equipmentDef = def;
            }
            else
            {
                ScrapyardLog.Error("Invalid AssetRequest " + request.assetName + " of type " + request.boxedAsset.GetType());
            }
        }


        //If an asset collection was loaded, the asset collection will be added to your mod's ContentPack.
        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public abstract bool Execute(EquipmentSlot slot);       
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract void OnEquipmentObtained(CharacterBody body);
    }
}