using FortunesFromTheScrapyard.Modules;
using RoR2.ContentManagement;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Path = System.IO.Path;

namespace FortunesFromTheScrapyard
{
    public static class FortunesAssets
    {
        public static AssetBundle mainAssetBundle;
        //the filename of your assetbundle
        internal static string assetBundleName = "fortunesassets";

        internal static string assemblyDir
        {
            get
            {
                return Path.GetDirectoryName(FortunesPlugin.instance.Info.Location);
            }
        }

        public static void PopulateAssets()
        {
            mainAssetBundle = Assets.LoadAssetBundle(assetBundleName);
            FortunesContent.serializedContentPack = mainAssetBundle.LoadAsset<SerializableContentPack>(FortunesContent.contentPackName);
        }
    }

    public class FortunesContent : IContentPackProvider
    {
        public static SerializableContentPack serializedContentPack;
        public static ContentPack contentPack;

        public static string contentPackName = "FortunesContentPack";

        public string identifier
        {
            get
            {
                return "FortunesFromTheScrapyard";
            }
        }

        internal static void Initialize()
        {
            contentPack = serializedContentPack.CreateContentPack();
            ContentManager.collectContentPackProviders += AddCustomContent;
        }

        private static void AddCustomContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new FortunesContent());
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
