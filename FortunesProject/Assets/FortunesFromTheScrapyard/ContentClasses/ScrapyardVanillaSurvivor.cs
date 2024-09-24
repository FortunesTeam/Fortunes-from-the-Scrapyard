using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortunesFromTheScrapyard
{
    public abstract class ScrapyardVanillaSurvivor : IContentPiece, IContentPackModifier
    {
        public abstract ScrapyardAssetRequest<AssetCollection> AssetRequest { get; }

        public AssetCollection survivorAssetCollection;
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            var assetRequest = AssetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.isComplete)
            {
                yield return null;
            }

            survivorAssetCollection = assetRequest.asset;

            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(survivorAssetCollection);
        }
    }
}