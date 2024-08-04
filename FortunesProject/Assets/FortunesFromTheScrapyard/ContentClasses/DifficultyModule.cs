using MSU;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortunesFromTheScrapyard
{
    public static class DifficultyModule
    {
        public static ReadOnlyDictionary<DifficultyDef, IDifficultyContentPiece> ScrapyardDifficulties { get; private set; }
        private static Dictionary<DifficultyDef, IDifficultyContentPiece> _scrapyardDifficulties = new Dictionary<DifficultyDef, IDifficultyContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static IContentPieceProvider<SerializableDifficultyDef> _contentPieceProvider;

        internal static IEnumerator Init()
        {
            if (moduleAvailability.available)
                yield break;

            _contentPieceProvider = ContentUtil.CreateContentPieceProvider<SerializableDifficultyDef>(ScrapyardMain.instance, ScrapyardContent.scrapyardContentPack);

            var enumerator = InitializeDifficultiesFromScrapyard();
            while (!enumerator.IsDone())
                yield return null;

            ScrapyardDifficulties = new ReadOnlyDictionary<DifficultyDef, IDifficultyContentPiece>(_scrapyardDifficulties);
            _scrapyardDifficulties = null;

            moduleAvailability.MakeAvailable();

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private static IEnumerator InitializeDifficultiesFromScrapyard()
        {
            IContentPiece<SerializableDifficultyDef>[] content = _contentPieceProvider.GetContents();

            List<IContentPiece<SerializableDifficultyDef>> difficulties = new List<IContentPiece<SerializableDifficultyDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach (var difficulty in content)
            {
                if (!difficulty.IsAvailable(_contentPieceProvider.ContentPack))
                    continue;

                difficulties.Add(difficulty);
                helper.Add(difficulty.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone)
                yield return null;

            InitializeDifficulties(difficulties);
        }

        private static void InitializeDifficulties(List<IContentPiece<SerializableDifficultyDef>> difficulties)
        {
            foreach (var difficulty in difficulties)
            {
                difficulty.Initialize();

                var asset = difficulty.Asset;
                DifficultyAPI.AddDifficulty(asset);

                if (difficulty is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(_contentPieceProvider.ContentPack);
                }
                if (difficulty is IDifficultyContentPiece diffContentPiece)
                {
                    _scrapyardDifficulties.Add(diffContentPiece.Asset.DifficultyDef, diffContentPiece);
                }
            }
        }

        private static void Run_onRunDestroyGlobal(Run obj)
        {
            var index = obj.selectedDifficulty;
            var def = DifficultyCatalog.GetDifficultyDef(index);

            if (ScrapyardDifficulties.TryGetValue(def, out var difficulty))
            {
                difficulty.OnRunEnd(obj);
            }
        }

        private static void Run_onRunStartGlobal(Run obj)
        {
            var index = obj.selectedDifficulty;
            var def = DifficultyCatalog.GetDifficultyDef(index);

            if (ScrapyardDifficulties.TryGetValue(def, out var difficulty))
            {
                difficulty.OnRunStart(obj);
            }
        }
    }

    public interface IDifficultyContentPiece : IContentPiece<SerializableDifficultyDef>
    {
        void OnRunStart(Run run);

        void OnRunEnd(Run run);
    }
}
