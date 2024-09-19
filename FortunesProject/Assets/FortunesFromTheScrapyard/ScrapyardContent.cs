using RoR2.ExpansionManagement;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;

namespace FortunesFromTheScrapyard
{
    public class ScrapyardContent : IContentPackProvider
    {
        public string identifier => ScrapyardMain.GUID;
        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(scrapyardContentPack);
        internal static ContentPack scrapyardContentPack { get; } = new ContentPack();

        internal static ParallelMultiStartCoroutine _parallelPreLoadDispatchers = new ParallelMultiStartCoroutine();
        private static Func<IEnumerator>[] _loadDispatchers;
        internal static ParallelMultiStartCoroutine _parallelPostLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Action[] _fieldAssignDispatchers;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = ScrapyardAssets.Initialize();
            while (enumerator.MoveNext())
                yield return null;

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f));
                enumerator = _loadDispatchers[i]();

                while (enumerator?.MoveNext() ?? false) yield return null;
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                _fieldAssignDispatchers[i]();
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(scrapyardContentPack, args.output);
            args.ReportProgress(1f);
            yield return null;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        private static IEnumerator LoadFromAssetBundles()
        {
            yield break;
        }
        /*
        private IEnumerator LoadSoundBank()
        {
            ScrapyardAssetRequest<GameObject> request = ScrapyardAssets.LoadAssetAsync<GameObject>("ScrapyardSoundInitializer", ScrapyardBundle.Main);
            while (!request.isComplete)
                yield return null;

            UnityEngine.Object.Instantiate(request.asset);
            yield break;
        }
        */
        private IEnumerator AddExpansionDef()
        {
            ScrapyardAssetRequest<ExpansionDef> request = ScrapyardAssets.LoadAssetAsync<ExpansionDef>("ScrapyardExpansionDef", ScrapyardBundle.Main);
            while (!request.isComplete)
                yield return null;

            scrapyardContentPack.expansionDefs.AddSingle(request.asset);
            yield break;
        }
        internal ScrapyardContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            ScrapyardAssets.onScrapyardAssetsInitialized += () =>
            {
                _parallelPreLoadDispatchers.Add(AddExpansionDef);
            };
        }

        static ScrapyardContent()
        {
            ScrapyardMain main = ScrapyardMain.instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {   
                DifficultyModule.Init,
                () =>
                {
                    CharacterModule.AddProvider(main, ContentUtil.CreateGameObjectGenericContentPieceProvider<CharacterBody>(main, scrapyardContentPack));
                    return CharacterModule.InitializeCharacters(main);
                },
                () =>
                {
                    ItemModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ItemDef>(main, scrapyardContentPack));
                    return ItemModule.InitializeItems(main);
                },
                () =>
                {
                    EquipmentModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<EquipmentDef>(main, scrapyardContentPack));
                    return EquipmentModule.InitializeEquipments(main);
                },
                LoadFromAssetBundles
            };

            _fieldAssignDispatchers = new Action[]
            {
                () => ContentUtil.PopulateTypeFields(typeof(Items), scrapyardContentPack.itemDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Equipments), scrapyardContentPack.equipmentDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Buffs), scrapyardContentPack.buffDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Survivors), scrapyardContentPack.survivorDefs),
                () => ContentUtil.PopulateTypeFields(typeof(NetworkedBodyAttachments), scrapyardContentPack.networkedObjectPrefabs),
                () => ContentUtil.PopulateTypeFields(typeof(NetworkSoundEventDefs), scrapyardContentPack.networkSoundEventDefs),
            };
        }
        public static class NetworkSoundEventDefs
        {
            public static NetworkSoundEventDef nsedScrapEliteSpawn;
            public static NetworkSoundEventDef nsedEnergyBar;
            public static NetworkSoundEventDef nsedMoonshineUse;
            public static NetworkSoundEventDef nsedDuctTapeActive;
            public static NetworkSoundEventDef nsedFaultyTurbo;
        }
        public static class NetworkedBodyAttachments
        {
        }
        public static class Items
        {
            public static ItemDef Headphones;
            public static ItemDef LethalInjection;
            public static ItemDef Multitool;
            public static ItemDef MultitoolConsumed;
            public static ItemDef SprayCan;
            public static ItemDef SprayCanConsumed;
            public static ItemDef Takeout;
            public static ItemDef CounterfeitCurrency;
            public static ItemDef FaultyTurbo;
            public static ItemDef OldCD;
            public static ItemDef DuctTape;
            public static ItemDef Polypore;
        }

        public static class Equipments
        {
            public static EquipmentDef EnergyBar;
            public static EquipmentDef MoonshineFlask;
            public static EquipmentDef EliteScrapEquipment;
        }

        public static class ExtendedEliteDefs
        {
            public static ExtendedEliteDef edScrap; 
        }
        public static class Buffs
        {
            public static BuffDef bdEnergyBar;
            public static BuffDef bdDisorient;
            public static BuffDef bdSprayCanCooldown;
            public static BuffDef bdSprayCanReady;
            public static BuffDef bdTakeoutDmg;
            public static BuffDef bdTakeoutSpeed;
            public static BuffDef bdTakeoutRegen;
            public static BuffDef bdFaultyTurbo;
            public static BuffDef bdLethalInjection;
            public static BuffDef bdDuctTape;
            public static BuffDef bdMoonshineFlask;
            public static BuffDef bdMoonshineStack;
            public static BuffDef bdChicken;
            public static BuffDef bdChickenCooldown;
            public static BuffDef bdPotstickers;
            public static BuffDef bdNoodles;
            public static BuffDef bdEliteScrap;
            public static BuffDef bdTimeStopped;
            public static BuffDef bdTimeSlow;
            public static BuffDef bdPolypore;
            public static BuffDef bdCloakerMarked;
            public static BuffDef bdCloakerMarkCd;
        }

        public static class Survivors
        {
            public static SurvivorDef Predator;
            public static SurvivorDef Neuromancer;
            public static SurvivorDef Badger;
            public static SurvivorDef Cloaker;
        }
    }
}