using MSU;
using R2API;
using R2API.AddressReferencedAssets;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoR2.CombatDirector;

namespace FortunesFromTheScrapyard
{
    public static class EliteTierModule
    {
        public static ReadOnlyDictionary<EliteTierDef, IEliteTierContentPiece> ScrapyardEliteTierDefs { get; private set; }
        private static Dictionary<EliteTierDef, IEliteTierContentPiece> scrapyardEliteTierDefs = new Dictionary<EliteTierDef, IEliteTierContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static IContentPieceProvider<SerializableEliteTierDef> _contentPieceProvider;

        private static List<IContentPiece<SerializableEliteTierDef>> eliteTiers;
        internal static IEnumerator Init()
        {
            if (moduleAvailability.available)
                yield break;

            _contentPieceProvider = ContentUtil.CreateGenericContentPieceProvider<SerializableEliteTierDef>(ScrapyardMain.instance, ScrapyardContent.scrapyardContentPack);

            var enumerator = InitializeEliteTiersFromScrapyard();
            while (!enumerator.IsDone())
                yield return null;

            ScrapyardEliteTierDefs = new ReadOnlyDictionary<EliteTierDef, IEliteTierContentPiece>(scrapyardEliteTierDefs);
            scrapyardEliteTierDefs = null;

            moduleAvailability.MakeAvailable();
        }

        private static IEnumerator InitializeEliteTiersFromScrapyard()
        {
            IContentPiece<SerializableEliteTierDef>[] content = _contentPieceProvider.GetContents();

            List<IContentPiece<SerializableEliteTierDef>> eliteTiers = new List<IContentPiece<SerializableEliteTierDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach (var eliteTier in content)
            {
                if (!eliteTier.IsAvailable(_contentPieceProvider.contentPack))
                    continue;

                eliteTiers.Add(eliteTier);
                helper.Add(eliteTier.LoadContentAsync);
            }

            helper.Start();
            while (!helper.isDone)
                yield return null;

            InitializeEliteTiers(eliteTiers);
        }

        private static void InitializeEliteTiers(List<IContentPiece<SerializableEliteTierDef>> eliteTiers)
        {
            EliteTierModule.eliteTiers = eliteTiers;

            AddressReferencedAsset.OnAddressReferencedAssetsLoaded += AddressReferencedAsset_OnAddressReferencedAssetsLoaded;
        }

        private static void AddressReferencedAsset_OnAddressReferencedAssetsLoaded()
        {
            foreach (var eliteTier in eliteTiers)
            {
                eliteTier.Initialize();

                eliteTier.asset.Init();

                if (eliteTier is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(_contentPieceProvider.contentPack);
                }
                if (eliteTier is IEliteTierContentPiece eliteTierContentPiece)
                {
                    scrapyardEliteTierDefs.Add(eliteTierContentPiece.asset.eliteTierDef, eliteTierContentPiece);
                }
            }
        }
    }

    public interface IEliteTierContentPiece : IContentPiece<SerializableEliteTierDef>
    {
        Func<SpawnCard.EliteRules, bool> isAvailableCheck { get; set; }
    }
}
