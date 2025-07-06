using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;

/*
 * This file contains all the code necesary for loading, managing and utilizing your mod's Assets and Asset Bundles.
 * 
 * Due to how unity's asset bundle building works, it is recommended to have multiple AssetBundles for your mod, this 
 * class manages loading assets from the multiple bundles setup instead of a singular bundle setup.
 */

namespace FortunesFromTheScrapyard
{
    /// <summary>
    /// Outside of Invalid, StreamedScene and All, each entry in this enum represents a non StreamedScene asset bundle 
    /// from your mod.
    /// </summary>
    public enum FFTSBundle
    {
        /// <summary>
        /// Represents an Invalid bundle, this is usually returned by the internal "FindAsset" type methods.
        /// </summary>
        Invalid,
        /// <summary>
        /// Represents a Streamed Scene bundle, this is only used during AssetBundle loading and using this value in other methods returns early.
        /// </summary>
        StreamedScene,
        /// <summary>
        /// A special enum value, if supplied on <see cref="FFTSAssets.LoadAsset{TAsset}(string, FFTSBundle)"/> or <see cref="FFTSAssets.LoadAllAssets{TAsset}(FFTSBundle)"/>, it'll search all the loaded asset bundles and return the first match or a collection of matches. This also applies to their Async counterparts.
        /// </summary>
        All,
        Main,
        Equipments,
        Items,
        Indev,
        Base,
        Elites,
        Survivors,
        Monsters,
        Difficulties
    }
    /// <summary>
    /// The mod's "Assets" class, which contains all the necesary assetbundle data for loading assets.
    /// </summary>
    public static class FFTSAssets
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";
        /*
         * If you decide to add a new AssetBundle (for example, an AssetBundle for Artifacts). Make sure to add a new
         * entry to these constants. The value of each constant is the Assetbundle name as specified in your mod's 
         * Manifest.
         */
        private const string MAIN = "fftsmain";
        private const string EQUIPMENTS = "fftsequipments";
        private const string ITEMS = "fftsitems";
        private const string INDEV = "fftsindev";
        private const string BASE = "fftsbase";
        private const string ELITES = "fftselites";
        private const string SURVIVORS = "fftssurvivors";
        private const string MONSTERS = "fftsmonsters";
        private const string DIFFICULTIES = "fftsdifficulties";
        private static string AssetBundleFolderPath => Path.Combine(Path.GetDirectoryName(FFTSMain.instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        private static Dictionary<FFTSBundle, AssetBundle> _assetBundles = new Dictionary<FFTSBundle, AssetBundle>();
        private static AssetBundle[] _streamedSceneBundles = Array.Empty<AssetBundle>();

        /// <summary>
        /// Fired when all the AssetBundles from the mod are loaded into memory, this in turn gets fired during Content Pack Loading and as such should be used to implement new async loading calls to <see cref="FFTSContent._parallelPreLoadDispatchers"/>
        /// </summary>
        public static event Action onScrapyardAssetsInitialized
        {
            add
            {
                _onExampleAssetsInitialized -= value;
                _onExampleAssetsInitialized += value;
            }
            remove
            {
                _onExampleAssetsInitialized -= value;
            }
        }
        private static Action _onExampleAssetsInitialized;

        /// <summary>
        /// Returns the AssetBundle that's tied to the supplied enum value.
        /// </summary>
        /// <param name="bundle">The bundle to obtain</param>
        /// <returns>The tied assetbundle, null if the enum value is All, Invalid or Streamed Scene</returns>
        public static AssetBundle GetAssetBundle(FFTSBundle bundle)
        {
            if (bundle == FFTSBundle.All || bundle == FFTSBundle.Invalid || bundle == FFTSBundle.StreamedScene)
            {
                return null;
            }

            return _assetBundles[bundle];
        }

        /// <summary>
        /// Loads an asset of type <typeparamref name="TAsset"/> and name <paramref name="name"/> from the asset bundle specified by <paramref name="bundle"/>.
        /// <para>See also <see cref="LoadAssetAsync{TAsset}(string, FFTSBundle)"/></para>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset</typeparam>
        /// <param name="name">The name of the Asset</param>
        /// <param name="bundle">The bundle to load from. Accepts the value <see cref="FFTSBundle.All"/>, but it'll log a warning since using the value <see cref="FFTSBundle.All"/> creates unecesary calls.</param>
        /// <returns>The loaded asset if it exists, null otherwise.</returns>
        public static TAsset LoadAsset<TAsset>(string name, FFTSBundle bundle) where TAsset : UObject
        {
            TAsset asset = null;
            if (bundle == FFTSBundle.All)
            {
                return FindAsset<TAsset>(name);
            }

            asset = _assetBundles[bundle].LoadAsset<TAsset>(name);

#if DEBUG
            if (!asset)
            {
                FFTSLog.Warning($"The method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, {nameof(FFTSBundle)})\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

                return LoadAsset<TAsset>(name, FFTSBundle.All);
            }
#endif
            return asset;
        }

        /// <summary>
        /// Creates an instance of <see cref="FFTSAssetRequest{TAsset}"/> which will contain the necesary metadata for loading an Asset asynchronously.
        /// <para>See also <see cref="LoadAsset{TAsset}(string, FFTSBundle)"/></para>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to load</typeparam>
        /// <param name="name">The name of the asset to load</param>
        /// <param name="bundle">The bundle to search thru, accepts the <see cref="FFTSBundle.All"/> value but it's not recommended as it creates unecesary calls.</param>
        /// <returns>The <see cref="FFTSAssetRequest{TAsset}"/> to use for asynchronous loading.</returns>
        public static FFTSAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name, FFTSBundle bundle) where TAsset : UObject
        {
            return new FFTSAssetRequest<TAsset>(name, bundle);
        }

        /// <summary>
        /// Loads all assets of type <typeparamref name="TAsset"/> from the AssetBundle specified by <paramref name="bundle"/>
        /// <para>See also <see cref="LoadAllAssetsAsync{TAsset}(FFTSBundle)"/></para>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to load</typeparam>
        /// <param name="bundle">The AssetBundle to load from, accepts the <see cref="FFTSBundle.All"/> value</param>
        /// <returns>An array of <typeparamref name="TAsset"/> which contains all the loaded assets.</returns>
        public static TAsset[] LoadAllAssets<TAsset>(FFTSBundle bundle) where TAsset : UObject
        {
            TAsset[] loadedAssets = null;
            if (bundle == FFTSBundle.All)
            {
                return FindAssets<TAsset>();
            }
            loadedAssets = _assetBundles[bundle].LoadAllAssets<TAsset>();

#if DEBUG
            if (loadedAssets.Length == 0)
            {
                FFTSLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} inside the bundle {bundle}");
            }
#endif
            return loadedAssets;
        }

        /// <summary>
        /// Creates an instance of <see cref="FFTSAssetRequest{TAsset}"/> which will contain the necesary metadata for loading an Asset asynchronously.
        /// <para>See also <see cref="LoadAllAssets{TAsset}(FFTSBundle)"/></para>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset to load</typeparam>
        /// <param name="bundle">The AssetBundle to load from, accepts the <see cref="FFTSBundle.All"/> value</param>
        /// <returns>The <see cref="FFTSAssetRequest{TAsset}"/> to use for asynchronous loading.</returns>
        public static FFTSAssetRequest<TAsset> LoadAllAssetsAsync<TAsset>(FFTSBundle bundle) where TAsset : UObject
        {
            return new FFTSAssetRequest<TAsset>(bundle);
        }

        /// <summary>
        /// Initializes the mod's asset bundles asynchronously, should only be called once and during <see cref="FFTSContent.LoadStaticContentAsync(RoR2.ContentManagement.LoadStaticContentAsyncArgs)"/>
        /// </summary>
        /// <returns>A coroutine which can be awaited.</returns>
        internal static IEnumerator Initialize()
        {
            FFTSLog.Info($"Initializing Assets...");
            var loadRoutine = LoadAssetBundles();

            while (!loadRoutine.IsDone())
            {
                yield return null;
            }

            //We can swap shaders in parallel
            ParallelMultiStartCoroutine multiStartCoroutine = new ParallelMultiStartCoroutine();
            multiStartCoroutine.Add(SwapShaders);
            multiStartCoroutine.Add(SwapAddressableShaders);

            while (!multiStartCoroutine.isDone) yield return null;

            _onExampleAssetsInitialized?.Invoke();
            yield break;
        }

        //This is a method which is used to load the AssetBundles from the mod asynchronously, it is very complicated but this method should not be touched as if you properly add the new Enum and const string values, managing the new bundles will be easy.
        //look at the method "LoadFromPath", that one contains stuff you should be interested in modifying in the future.
        private static IEnumerator LoadAssetBundles()
        {
            ParallelMultiStartCoroutine helper = new ParallelMultiStartCoroutine();

            List<(string path, FFTSBundle bundleEnum, AssetBundle loadedBundle)> pathsAndBundles = new List<(string path, FFTSBundle bundleEnum, AssetBundle loadedBundle)>();

            string[] paths = GetAssetBundlePaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                helper.Add(LoadFromPath, pathsAndBundles, path, i, paths.Length);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            foreach ((string path, FFTSBundle bundleEnum, AssetBundle assetBundle) in pathsAndBundles)
            {
                if (bundleEnum == FFTSBundle.StreamedScene)
                {
                    HG.ArrayUtils.ArrayAppend(ref _streamedSceneBundles, assetBundle);
                }
                else
                {
                    _assetBundles[bundleEnum] = assetBundle;
                }
            }
        }

        //This method is what actually loads the AssetBundle into memory, and assigns it a Bundle enum value if needed.
        private static IEnumerator LoadFromPath(List<(string path, FFTSBundle bundleEnum, AssetBundle loadedBundle)> list, string path, int index, int totalPaths)
        {
            string fileName = Path.GetFileName(path);
            FFTSBundle? exampleBundle = null;
            //FFTSLog.Warning(fileName);
            if (fileName == "AssetBundles")
            {
                yield break;
            }
            //When you add new AssetBundles, you should add new Cases to this switch clause for your new bundles, for example, if you where to add an "Artifacts" bundle, you'd write the following line (which is commented in this scenario.) this is all you need to do to get new asset bundles loading.
            switch (fileName)
            {
                case MAIN: exampleBundle = FFTSBundle.Main; break;
                case EQUIPMENTS: exampleBundle = FFTSBundle.Equipments; break;
                case ITEMS: exampleBundle = FFTSBundle.Items; break;
                case DIFFICULTIES: exampleBundle = FFTSBundle.Difficulties; break;
                case INDEV: exampleBundle = FFTSBundle.Indev; break;
                case BASE: exampleBundle = FFTSBundle.Base; break;
                case ELITES: exampleBundle = FFTSBundle.Elites; break;
                case SURVIVORS: exampleBundle = FFTSBundle.Survivors; break;
                case MONSTERS: exampleBundle = FFTSBundle.Monsters; break;

                //This path does not match any of the non scene bundles, could be a scene, we will mark these on only this ocassion as "StreamedScene".
                default: exampleBundle = FFTSBundle.StreamedScene; break;
            }
           
            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                yield return null;
            }

            AssetBundle bundle = request.assetBundle;

            //Throw if no bundle was loaded
            if (!bundle)
            {
                throw new FileLoadException($"AssetBundle.LoadFromFile did not return an asset bundle. (Path={path})");
            }

            //The switch statement considered this a streamed scene bundle
            if (exampleBundle == FFTSBundle.StreamedScene)
            {
                //supposed bundle is not streamed scene? throw exception.
                if (!bundle.isStreamedSceneAssetBundle)
                {
                    throw new Exception($"AssetBundle in specified path is not a streamed scene bundle, but its file name was not found in the Switch statement. have you forgotten to setup the enum and file name in your assets class? (Path={path})");
                }
                else
                {
                    //bundle is streamed scene, add to the list and break.
                    list.Add((path, FFTSBundle.StreamedScene, bundle));
                    yield break;
                }
            }

            //The switch statement considered this to not be a streamed scene bundle, but an assets bundle.
            list.Add((path, exampleBundle.Value, bundle));
            yield break;
        }

        private static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(AssetBundleFolderPath).Where(filePath => !filePath.EndsWith(".manifest")).ToArray();
        }

        //Utilize the built in "ShaderUtil" class from MSU to swap both kinds of shaders.
        private static IEnumerator SwapShaders()
        {
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundles.Values.ToArray());
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundles.Values.ToArray());
        }

        //This method tries to find an asset of type TAsset and of a specific name in all the bundles, it returns the first match.
        //There's usually no need to run this method in Release builds, and it mostly exists for Development purposes.
        private static TAsset FindAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            TAsset loadedAsset = null;
            FFTSBundle foundInBundle = FFTSBundle.Invalid;
            foreach ((var enumVal, var assetBundle) in _assetBundles)
            {
                loadedAsset = assetBundle.LoadAsset<TAsset>(name);

                if (loadedAsset)
                {
                    foundInBundle = enumVal;
                    break;
                }
            }

#if DEBUG
            if (loadedAsset)
                FFTSLog.Info($"Asset of type {typeof(TAsset).Name} with name {name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly.");
            else
                FFTSLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
#endif

            return loadedAsset;
        }

        //This method tries to find all assets of type TAsset in all the bundles, it returns a collection of assets.
        private static TAsset[] FindAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            List<TAsset> assets = new List<TAsset>();
            foreach ((_, var bundles) in _assetBundles)
            {
                assets.AddRange(bundles.LoadAllAssets<TAsset>());
            }

#if DEBUG
            if (assets.Count == 0)
                FFTSLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
#endif

            return assets.ToArray();
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for (int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(FFTSAssets))
                    continue;

                if (declaringType == typeof(FFTSAssets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
            }
            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif
    }

    /// <summary>
    /// A class that represents a request for loading Assets asynchronously.
    /// <br>You're strongly advised to use and check out <see cref="FFTSAssetRequest{TAsset}"/> instead.</br>
    /// </summary>
    public abstract class FFTSAssetRequest
    {
        /// <summary>
        /// The loaded asset, boxed as a Unity Object.
        /// </summary>
        public abstract UObject boxedAsset { get; }

        /// <summary>
        /// The loaded assets, boxed as an Enumerable of Unity Object
        /// </summary>
        public abstract IEnumerable<UObject> boxedAssets { get; }

        /// <summary>
        /// The AssetBundle to load from.
        /// </summary>
        public FFTSBundle targetBundle => _targetBundle;
        private FFTSBundle _targetBundle;

        /// <summary>
        /// The name of the asset to load. Can be null in the scenario this request loads multiple assets.
        /// </summary>
        public NullableRef<string> assetName => _assetName;
        private NullableRef<string> _assetName;

        /// <summary>
        /// Wether this request is loading a single asset, or multiple assets.
        /// </summary>
        public bool singleAssetLoad { get; private set; }
        /// <summary>
        /// Checks if the asynchronous loading operation has completed.
        /// </summary>
        public bool isComplete
        {
            get
            {
                if (internalCoroutine == null)
                    StartLoad();

                return !internalCoroutine.MoveNext();
            }
        }

        /// <summary>
        /// The coroutine that's loading the assets
        /// </summary>
        protected IEnumerator internalCoroutine;

        /// <summary>
        /// The AssetType's Name
        /// </summary>
        protected string assetTypeName;

        /// <summary>
        /// Starts the loading coroutine from this AssetRequest.
        /// </summary>
        public void StartLoad()
        {
            if (singleAssetLoad)
            {
                internalCoroutine = LoadSingleAsset();
            }
            else
            {
                internalCoroutine = LoadMultipleAsset();
            }
        }

        /// <summary>
        /// Implement the method that loads a Single asset asynchronously.
        /// </summary>
        /// <returns>A coroutine</returns>
        protected abstract IEnumerator LoadSingleAsset();

        /// <summary>
        /// Implement the method that loads multiple assets asynchronously.
        /// </summary>
        /// <returns>A coroutine</returns>
        protected abstract IEnumerator LoadMultipleAsset();

        /// <summary>
        /// Constructor for an ExampleAssetRequest that'll load a single asset
        /// </summary>
        /// <param name="assetName">The name of the asset</param>
        /// <param name="bundleEnum">The AssetBundle to load from, accepts the value <see cref="FFTSBundle.All"/>, but it shouldn't be used as it generates unecesary overhead</param>
        public FFTSAssetRequest(string assetName, FFTSBundle bundleEnum)
        {
            _assetName = assetName;
            _targetBundle = bundleEnum;
            singleAssetLoad = true;
            assetTypeName = "UnityEngine.Object";
        }

        /// <summary>
        /// Constructor for an ExampleAssetRequest that'll load multiple assets
        /// </summary>
        /// <param name="bundleEnum">The AssetBundle to load from, accepts the value <see cref="FFTSBundle.All"/></param>
        public FFTSAssetRequest(FFTSBundle bundleEnum)
        {
            _assetName = string.Empty;
            _targetBundle = bundleEnum;
            singleAssetLoad = false;
            assetTypeName = "UnityEngine.Object";
        }
    }

    public class FFTSAssetRequest<TAsset> : FFTSAssetRequest where TAsset : UObject
    {
        public override UObject boxedAsset => _asset;
        public TAsset asset => _asset;
        private TAsset _asset;

        public override IEnumerable<UObject> boxedAssets => _assets;
        public IEnumerable<TAsset> assets => _assets;
        private List<TAsset> _assets;

        protected override IEnumerator LoadSingleAsset()
        {
            AssetBundleRequest request = null;

            request = FFTSAssets.GetAssetBundle(targetBundle).LoadAssetAsync<TAsset>(assetName); ;
            while (!request.isDone)
                yield return null;

            _asset = (TAsset)request.asset;

#if DEBUG
            //Asset found, dont try to find it.
            if (_asset)
                yield break;

            FFTSLog.Warning($"The method \"{GetCallingMethod()}\" is calling a ScrapyardAssetRequest.StartLoad() while the class has the values \"{assetTypeName}\", \"{assetName}\" and \"{targetBundle}\", however, the asset could not be found.\n" +
    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

            FFTSBundle foundInBundle = FFTSBundle.Invalid;
            foreach (FFTSBundle bundleEnum in Enum.GetValues(typeof(FFTSBundle)))
            {
                if (bundleEnum == FFTSBundle.All || bundleEnum == FFTSBundle.Invalid || bundleEnum == FFTSBundle.StreamedScene)
                    continue;

                request = FFTSAssets.GetAssetBundle(bundleEnum).LoadAssetAsync<TAsset>(assetName);
                while (!request.isDone)
                {
                    yield return null;
                }

                if (request.asset)
                {
                    _asset = (TAsset)request.asset;
                    foundInBundle = bundleEnum;
                    break;
                }
            }

            if (_asset)
            {
                FFTSLog.Info($"Asset of type {assetTypeName} and name {assetName} was found inside bundle {foundInBundle}. It is recommended to load the asset directly.");
            }
            else
            {
                FFTSLog.Fatal($"Could not find asset of type {assetTypeName} and name {assetName} In any of the bundles, exceptions may occur.");
            }
#endif
            yield break;
        }

        protected override IEnumerator LoadMultipleAsset()
        {
            _assets.Clear();

            AssetBundleRequest request = null;
            if (targetBundle == FFTSBundle.All)
            {
                foreach (FFTSBundle enumVal in Enum.GetValues(typeof(FFTSBundle)))
                {
                    if (enumVal == FFTSBundle.All || enumVal == FFTSBundle.Invalid || enumVal == FFTSBundle.StreamedScene)
                        continue;

                    request = FFTSAssets.GetAssetBundle(targetBundle).LoadAllAssetsAsync<TAsset>();
                    while (!request.isDone)
                        yield return null;

                    _assets.AddRange(request.allAssets.OfType<TAsset>());
                }

#if DEBUG
                if (_assets.Count == 0)
                {
                    FFTSLog.Warning($"Could not find any asset of type {assetTypeName} in any of the bundles");
                }
#endif
                yield break;
            }

            request = FFTSAssets.GetAssetBundle(targetBundle).LoadAllAssetsAsync<TAsset>();
            while (!request.isDone) yield return null;

            _assets.AddRange(request.allAssets.OfType<TAsset>());

#if DEBUG
            if (_assets.Count == 0)
            {
                FFTSLog.Warning($"Could not find any asset of type {assetTypeName} inside the bundle {targetBundle}");
            }
#endif

            yield break;
        }

#if DEBUG
        private static string GetCallingMethod()
        {
            var stackTrace = new StackTrace();

            for (int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)
            {
                var frame = stackTrace.GetFrame(stackFrameIndex);
                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(FFTSAssets))
                    continue;

                if (declaringType == typeof(FFTSAssets))
                    continue;

                var fileName = frame.GetFileName();
                var fileLineNumber = frame.GetFileLineNumber();
                var fileColumnNumber = frame.GetFileColumnNumber();

                return $"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})";
            }
            return "[COULD NOT GET CALLING METHOD]";
        }

        private static string GetMethodParams(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            if (parameters.Length == 0)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ToString() + ", ");
            }
            return stringBuilder.ToString();
        }
#endif

        internal FFTSAssetRequest(string name, FFTSBundle bundle) : base(name, bundle)
        {
            assetTypeName = typeof(TAsset).Name;
        }

        internal FFTSAssetRequest(FFTSBundle bundle) : base(bundle)
        {
            _assets = new List<TAsset>();
            assetTypeName = typeof(TAsset).Name;
        }
    }
}