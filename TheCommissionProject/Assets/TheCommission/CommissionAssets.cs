using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MSU;
using System.Collections;
using RoR2.ContentManagement;
using RoR2;
using Path = System.IO.Path;
using System.Diagnostics;
using System.Reflection;
using UObject = UnityEngine.Object;
using System.Xml.Linq;
using R2API.Utils;

namespace TheCommission
{
    public enum CommissionBundle
    {
        Invalid,
        All,
        Main,
    }

    public static class CommissionAssets
    {
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";

        private const string MAIN = "tcmain";

        private static string AssetBundleFolderPath => Path.Combine(Path.GetDirectoryName(Main.Instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);
        private static Dictionary<CommissionBundle, AssetBundle> _assetBundles = new Dictionary<CommissionBundle, AssetBundle>();
        private static AssetBundle[] _streamedSceneAssetBundles = Array.Empty<AssetBundle>();

        public static AssetBundle GetAssetBundle(CommissionBundle bundle)
        {
            return _assetBundles[bundle];
        }

        public static TAsset LoadAsset<TAsset>(string name, CommissionBundle bundle) where TAsset : UObject
        {
            TAsset asset = null;
            if (bundle == CommissionBundle.All)
            {
                return FindAsset<TAsset>(name);
            }

            asset = _assetBundles[bundle].LoadAsset<TAsset>(name);

#if DEBUG
            if (!asset)
            {
                TCLog.Warning($"The method \"{GetCallingMethod()}\" is calling \"LoadAsset<TAsset>(string, CommissionBundle)\" with the arguments \"{typeof(TAsset).Name}\", \"{name}\" and \"{bundle}\", however, the asset could not be found.\n" +
                    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

                return LoadAsset<TAsset>(name, CommissionBundle.All);
            }
#endif
            return asset;
        }

        public static CommissionAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name, CommissionBundle bundle) where TAsset : UObject
        {
            return new CommissionAssetRequest<TAsset>(name, bundle);
        }

        public static TAsset[] LoadAllAssets<TAsset>(CommissionBundle bundle) where TAsset : UObject
        {
            TAsset[] loadedAssets = null;
            if (bundle == CommissionBundle.All)
            {
                return FindAssets<TAsset>();
            }
            loadedAssets = _assetBundles[bundle].LoadAllAssets<TAsset>();

#if DEBUG
            if (loadedAssets.Length == 0)
            {
                TCLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} inside the bundle {bundle}");
            }
#endif
            return loadedAssets;
        }

        public static CommissionAssetRequest<TAsset> LoadAssetsAsync<TAsset>(CommissionBundle bundle) where TAsset : UObject
        {
            return new CommissionAssetRequest<TAsset>(bundle);
        }

        internal static IEnumerator Initialize()
        {
            TCLog.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundles();
            while (loadRoutine.MoveNext()) yield return null;

            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();
            helper.Add(SwapShaders);
            helper.Add(SwapAddressableShaders);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            yield break;
        }

        private static IEnumerator LoadAssetBundles()
        {
            ParallelCoroutineHelper helper = new ParallelCoroutineHelper();

            List<(string path, CommissionBundle bundleEnum, AssetBundle loadedBundle)> pathsAndBundles = new List<(string path, CommissionBundle bundleEnum, AssetBundle loadedBundle)>();

            string[] paths = GetAssetBundlePaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                helper.Add(LoadFromPath, pathsAndBundles, path, i, paths.Length);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            foreach((string path, CommissionBundle bundleEnum, AssetBundle assetBundle) in pathsAndBundles)
            {
                if(bundleEnum == CommissionBundle.Invalid)
                {
                    HG.ArrayUtils.ArrayAppend(ref _streamedSceneAssetBundles, assetBundle);
                }
                else
                {
                    _assetBundles[bundleEnum] = assetBundle;
                }
            }
        }

        private static IEnumerator LoadFromPath(List<(string path, CommissionBundle bundleEnum, AssetBundle loadedBundle)> list, string path, int index, int totalPaths)
        {
            string fileName = Path.GetFileName(path);
            CommissionBundle? commissionBundleEnum = null;
            switch (fileName)
            {
                case MAIN: commissionBundleEnum = CommissionBundle.Main; break;
                //This path does not match any of the non scene bundles, could be a scene, we will mark these on only this ocassion as "Invalid".
                default: commissionBundleEnum = CommissionBundle.Invalid; break;
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
            if(commissionBundleEnum == CommissionBundle.Invalid)
            {
                //supposed bundle is not streamed scene? throw exception.
                if(!bundle.isStreamedSceneAssetBundle)
                {
                    throw new Exception($"AssetBundle in specified path is not a streamed scene bundle, but its file name was not found in the Switch statement. have you forgotten to setup the enum and file name in your assets class? (Path={path})");
                }
                else
                {
                    //bundle is streamed scene, add to the list and break.
                    list.Add((path, CommissionBundle.Invalid, bundle));
                    yield break;
                }
            }

            //The switch statement considered this to not be a streamed scene bundle, but an assets bundle.
            list.Add((path, commissionBundleEnum.Value, bundle));
            yield break;
        }

        private static IEnumerator SwapShaders()
        {
            List<AssetBundle> nonStreamedSceneBundles = new List<AssetBundle>();

            foreach(var (_, assetBundle) in _assetBundles)
            {
                if (assetBundle.isStreamedSceneAssetBundle)
                    continue;

                nonStreamedSceneBundles.Add(assetBundle);
            }

            return ShaderUtil.SwapAssetBundleShadersAsync(nonStreamedSceneBundles.ToArray());
        }

        private static IEnumerator SwapAddressableShaders()
        {
            List<AssetBundle> nonStreamedSceneBundles = new List<AssetBundle>();

            foreach(var (_, assetBundle) in _assetBundles)
            {
                if (assetBundle.isStreamedSceneAssetBundle)
                    continue;

                nonStreamedSceneBundles.Add(assetBundle);
            }

            return ShaderUtil.LoadAddressableMaterialShadersAsync(nonStreamedSceneBundles.ToArray());
        }

        private static TAsset FindAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            TAsset loadedAsset = null;
            CommissionBundle foundInBundle = CommissionBundle.Invalid;
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
                TCLog.Info($"Asset of type {typeof(TAsset).Name} with name {name} was found inside bundle {foundInBundle}, it is recommended that you load the asset directly.");
            else
                TCLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {name} in any of the bundles.");
#endif

            return loadedAsset;
        }

        private static TAsset[] FindAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            List<TAsset> assets = new List<TAsset>();
            foreach ((_, var bundles) in _assetBundles)
            {
                assets.AddRange(bundles.LoadAllAssets<TAsset>());
            }

#if DEBUG
            if (assets.Count == 0)
                TCLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
#endif

            return assets.ToArray();
        }

        private static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(AssetBundleFolderPath).Where(filePath => !filePath.EndsWith(".manifest")).ToArray();
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
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(CommissionAssets))
                    continue;

                if (declaringType == typeof(CommissionAssets))
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

    public class CommissionAssetRequest<TAsset> where TAsset : UnityEngine.Object
    {
        public TAsset Asset => _asset;
        private TAsset _asset;

        public IEnumerable<TAsset> Assets => _assets;
        private List<TAsset> _assets;

        public CommissionBundle TargetBundle => _targetBundle;
        private CommissionBundle _targetBundle;

        public NullableRef<string> AssetName => _assetName;
        private NullableRef<string> _assetName;

        private bool _singleAssetLoad = true;

        public bool IsComplete => !_internalCoroutine.MoveNext();
        private IEnumerator _internalCoroutine;

        public void StartLoad()
        {
            if (_singleAssetLoad)
            {
                _internalCoroutine = LoadSingleAsset();
            }
            else
            {
                _internalCoroutine = LoadMultipleAsset();
            }
        }

        private IEnumerator LoadSingleAsset()
        {
            AssetBundleRequest request = null;
            if (_targetBundle == CommissionBundle.All)
            {
                foreach (CommissionBundle enumVal in Enum.GetValues(typeof(CommissionBundle)))
                {
                    if (enumVal == CommissionBundle.Invalid || enumVal == CommissionBundle.All)
                        continue;

                    var bundle = CommissionAssets.GetAssetBundle(enumVal);
                    request = bundle.LoadAssetAsync<TAsset>(AssetName);
                    while (!request.isDone)
                    {
                        yield return null;
                    }

                    _asset = (TAsset)request.asset;
                    if (Asset)
                    {
                        _targetBundle = enumVal;
                        yield break;
                    }
                }

#if DEBUG
                if (!Asset)
                {
                    _targetBundle = CommissionBundle.Invalid;
                    TCLog.Warning($"Could not find asset of type {typeof(TAsset).Name} with name {AssetName} in any of the bundles.");
                }
                else
                {
                    TCLog.Info($"Asset of type {typeof(TAsset).Name} with name {AssetName} was found inside bundle {TargetBundle}, it is recommended that you load the asset directly.");
                }
#endif
                yield break;
            }

            request = CommissionAssets.GetAssetBundle(TargetBundle).LoadAssetAsync<TAsset>(AssetName); ;
            while (!request.isDone)
                yield return null;

            _asset = (TAsset)request.asset;

#if DEBUG
            TCLog.Warning($"The method \"{GetCallingMethod()}\" is calling a CommissionAssetRequest.StartLoad() while the class has the values \"{typeof(TAsset).Name}\", \"{AssetName}\" and \"{TargetBundle}\", however, the asset could not be found.\n" +
    $"A complete search of all the bundles will be done and the correct bundle enum will be logged.");

            _targetBundle = CommissionBundle.All;
            _internalCoroutine.Reset();
            yield break;
#endif
        }

        private IEnumerator LoadMultipleAsset()
        {
            _assets.Clear();

            AssetBundleRequest request = null;
            if (TargetBundle == CommissionBundle.All)
            {
                foreach (CommissionBundle enumVal in Enum.GetValues(typeof(CommissionBundle)))
                {
                    if (enumVal == CommissionBundle.All || enumVal == CommissionBundle.Invalid)
                        continue;

                    request = CommissionAssets.GetAssetBundle(enumVal).LoadAllAssetsAsync<TAsset>();
                    while (!request.isDone)
                        yield return null;

                    _assets.AddRange(request.allAssets.OfType<TAsset>());
                }

#if DEBUG
                if (_assets.Count == 0)
                {
                    TCLog.Warning($"Could not find any asset of type {typeof(TAsset).Name} in any of the bundles");
                }
#endif
                yield break;
            }

            request = CommissionAssets.GetAssetBundle(TargetBundle).LoadAllAssetsAsync<TAsset>();
            while(!request.isDone) yield return null;

            _assets.AddRange(request.allAssets.OfType<TAsset>());

#if DEBUG
            if(_assets.Count == 0)
            {
                TCLog.Warning($"Could not find any asset of type {typeof(TAsset)} inside the bundle {TargetBundle}");
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
                if (declaringType.IsGenericType && declaringType.DeclaringType == typeof(CommissionAssets))
                    continue;

                if (declaringType == typeof(CommissionAssets))
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

        internal CommissionAssetRequest(string name, CommissionBundle bundle)
        {
            _singleAssetLoad = true;
            _assetName = name;
            _targetBundle = bundle;
        }

        internal CommissionAssetRequest(CommissionBundle bundle)
        {
            _singleAssetLoad = false;
            _assetName = new NullableRef<string>();
            _assets = new List<TAsset>();
            _targetBundle = bundle;
        }
    }
}