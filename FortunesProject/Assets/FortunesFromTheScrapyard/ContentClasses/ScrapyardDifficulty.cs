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
    public abstract class ScrapyardDifficulty : IDifficultyContentPiece
    {
        public SerializableDifficultyDef difficultyDef { get; protected set; }

        SerializableDifficultyDef IContentPiece<SerializableDifficultyDef>.asset => difficultyDef;

        public abstract ScrapyardAssetRequest<SerializableDifficultyDef> AssetRequest { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            var assetRequest = AssetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.isComplete)
                yield return null;

            difficultyDef = assetRequest.asset;
            yield break;
        }
        public abstract void OnRunEnd(Run run);
        public abstract void OnRunStart(Run run);
    }
}
