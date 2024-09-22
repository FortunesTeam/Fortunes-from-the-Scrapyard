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
    public abstract class ScrapyardEliteTier : IEliteTierContentPiece
    {
        public SerializableEliteTierDef eliteTierDef { get; protected set; }

        SerializableEliteTierDef IContentPiece<SerializableEliteTierDef>.asset => eliteTierDef;
        public abstract ScrapyardAssetRequest<SerializableEliteTierDef> AssetRequest { get; }
        public Func<SpawnCard.EliteRules, bool> isAvailableCheck { get; set; }
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            var assetRequest = AssetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.isComplete)
                yield return null;

            eliteTierDef = assetRequest.asset;
            yield break;
        }
    }
}
