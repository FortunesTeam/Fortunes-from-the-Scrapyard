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
    /// <inheritdoc cref="IEliteContentPiece"/>
    /// </summary>
    public abstract class ScrapyardEliteEquipment : IEliteContentPiece, IContentPackModifier
    {
        /// <summary>
        /// <inheritdoc cref="IEliteContentPiece.EliteDefs"/>
        /// </summary>
        public List<EliteDef> eliteDefs { get; protected set; }
        /// <summary>
        /// The EliteAssetCollection for this Elite. Populated when the Elite's assets loads, cannot be null.
        /// </summary>
        public EliteAssetCollection assetCollection { get; private set; }
        /// <summary>
        /// <inheritdoc cref="IEquipmentContentPiece.ItemDisplayPrefabs"/>
        /// </summary>
        public NullableRef<List<GameObject>> itemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        /// <summary>
        /// <inheritdoc cref="IContentPiece{T}.Asset"/>
        /// </summary>
        public EquipmentDef equipmentDef { get; protected set; }

        List<EliteDef> IEliteContentPiece.eliteDefs => eliteDefs;
        NullableRef<List<GameObject>> IEquipmentContentPiece.itemDisplayPrefabs => itemDisplayPrefabs;
        EquipmentDef IContentPiece<EquipmentDef>.asset => equipmentDef;


        /// <summary>
        /// Method for loading an AssetRequest for this class. This will later get loaded Asynchronously.
        /// </summary>
        /// <returns>An ExampleAssetRequest that loads an EliteAssetCollection</returns>
        public abstract ScrapyardAssetRequest<EliteAssetCollection> LoadAssetRequest();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            ScrapyardAssetRequest<EliteAssetCollection> request = LoadAssetRequest();

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            assetCollection = request.asset;

            eliteDefs = assetCollection.eliteDefs;
            equipmentDef = assetCollection.equipmentDef;
            itemDisplayPrefabs = assetCollection.itemDisplayPrefabs;
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