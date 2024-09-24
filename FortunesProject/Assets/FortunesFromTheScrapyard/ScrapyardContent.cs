using RoR2.ExpansionManagement;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using EntityStates.FalseSonBoss;
using System.Collections.Generic;

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
            while (!_parallelPostLoadDispatchers.isDone) yield return null;

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
            ScrapyardLog.Info($"Populating EntityStateTypes array...");
            scrapyardContentPack.entityStateTypes.Clear();
            scrapyardContentPack.entityStateTypes.Add(typeof(ScrapyardContent).Assembly.GetTypes().Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type)).ToArray());

            /*
            ScrapyardLog.Info("Populating EntityStateConfiguration array...");
            ScrapyardAssetRequest<EntityStateConfiguration> escRequest = new ScrapyardAssetRequest<EntityStateConfiguration>(ScrapyardBundle.All);
            escRequest.StartLoad();
            while (!escRequest.isComplete) yield return null;
            scrapyardContentPack.entityStateConfigurations.Add(escRequest.assets.ToArray());
            
            ScrapyardLog.Info($"Populating EffectDefs array...");
            ScrapyardAssetRequest<GameObject> gameObjectRequest = new ScrapyardAssetRequest<GameObject>(ScrapyardBundle.All);
            gameObjectRequest.StartLoad();
            while (!gameObjectRequest.isComplete) yield return null;
            scrapyardContentPack.effectDefs.Add(gameObjectRequest.assets.Where(go => go.GetComponent<EffectComponent>()).Select(go => new EffectDef(go)).ToArray());
            */

            ScrapyardLog.Info($"Calling AsyncAssetLoad Attribute Methods...");
            ParallelMultiStartCoroutine asyncAssetLoadCoroutines = AsyncAssetLoadAttribute.CreateCoroutineForMod(ScrapyardMain.instance);
            asyncAssetLoadCoroutines.Start();
            while (!asyncAssetLoadCoroutines.isDone)
                yield return null;
        }
        private static IEnumerator LoadVanillaSurvivorBundles()
        {
            ParallelMultiStartCoroutine helper = new ParallelMultiStartCoroutine();

            var list = new List<ScrapyardVanillaSurvivor>()
              {
                //Add the rest
              };

            foreach (var survivor in list)
            {
                helper.Add(survivor.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
            {
                yield return null;
            }

            foreach (var survivor in list)
            {
                survivor.Initialize();
                survivor.ModifyContentPack(scrapyardContentPack);
            }
        }

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
                //EliteTierModule.Init,
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
            public static ItemDef RoughReception;
        }

        public static class Equipments
        {
            public static EquipmentDef EnergyBar;
            public static EquipmentDef MoonshineFlask;
            public static EquipmentDef EliteScrapEquipment;
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
            public static BuffDef bdDukeDamageShare;
            public static BuffDef bdDukeSpeedBuff;
            public static BuffDef bdDukeFreeShot;
            public static BuffDef bdCounterfeitLimit;
        }

        public static class Survivors
        {
            public static SurvivorDef Predator;
            public static SurvivorDef Neuromancer;
            public static SurvivorDef Pacer;
            public static SurvivorDef Cloaker;
            public static SurvivorDef Duke;
            public static SurvivorDef Skater;
        }

        public static void CreateAndAddEffectDef(GameObject effect)
        {
            EffectDef effectDef = new EffectDef(effect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef);
        }

        public static void AddNetworkSoundEventDef(NetworkSoundEventDef networkSoundEventDef)
        {
            ScrapyardContent.scrapyardContentPack.networkSoundEventDefs.AddSingle(networkSoundEventDef);
        }
        public static NetworkSoundEventDef CreateAndAddNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            AddNetworkSoundEventDef(networkSoundEventDef);

            return networkSoundEventDef;
        }
    }
}