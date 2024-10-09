using System;
using System.Collections;
using System.Linq;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using RoR2.Networking;
using R2API;

namespace FortunesFromTheScrapyard.Content
{
    public static class FortunesContent
    {
        internal const string ScenesAssetBundleFileName = "fortunesscrapyardscenes";
        internal const string AssetsAssetBundleFileName = "fortunesscrapyardstageassets";

        //internal const string MusicSoundBankFileName = "SnowtimeStagesMusic.bnk";
        //internal const string SndSoundBankFileName = "SnowtimeStagesSounds.bnk";
        //internal const string MusicSoundBankFileName = "SM64BBFMusic.bnk";
        //internal const string InitSoundBankFileName = "SnowtimeStagesInit.bnk";
        //internal const string InitSoundBankFileName = "SM64BBFInit.bnk";


        private static AssetBundle _scenesAssetBundle;
        private static AssetBundle _assetsAssetBundle;

        internal static UnlockableDef[] UnlockableDefs;
        internal static SceneDef[] SceneDefs;
        internal static ExpansionDef[] expansionDefs;
        // Halo Content
        internal static ExpansionDef ExpansionDefSTHalo;

        // STSceneDef = Death Island
        // STBGSceneDef = Blood Gulch
        // STGPHSceneDef = Gephyrophobia
        // STHSceneDef = Halo
        // STH2SceneDef = Halo
        // STIFSceneDef = Ice Fields
        // STShrineSceneDef = Sandtrap
        internal static SceneDef STSceneDef;
        internal static SceneDef STIFSceneDef;
        internal static SceneDef STBGSceneDef;
        internal static SceneDef STGPHSceneDef;
        internal static SceneDef STHSceneDef;
        internal static SceneDef STH2SceneDef;
        internal static SceneDef STShrineSceneDef;
        internal static Sprite STSceneDefPreviewSprite;
        internal static Sprite STIFSceneDefPreviewSprite;
        internal static Sprite STBGSceneDefPreviewSprite;
        internal static Sprite STGPHSceneDefPreviewSprite;
        internal static Sprite STHSceneDefPreviewSprite;
        internal static Sprite STH2SceneDefPreviewSprite;
        internal static Sprite STShrineSceneDefPreviewSprite;
       // internal static Material STBazaarSeer;
       // internal static Material STIFBazaarSeer;
       // internal static Material STBGBazaarSeer;
       // internal static Material STGPHBazaarSeer;
       // internal static Material STHBazaarSeer;
       // internal static Material STH2BazaarSeer;
       // internal static Material STShrineBazaarSeer;

        public static List<Material> SwappedMaterials = new List<Material>(); //debug

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "RoR2/Base/Shaders/HGStandard.shader"},
            {"stubbedror2/base/shaders/hgsnowtopped", "RoR2/Base/Shaders/HGSnowTopped.shader"},
            {"stubbedror2/base/shaders/hgtriplanarterrainblend", "RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader"},
            {"stubbedror2/base/shaders/hgintersectioncloudremap", "RoR2/Base/Shaders/HGIntersectionCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgcloudremap", "RoR2/Base/Shaders/HGCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgdistortion", "RoR2/Base/Shaders/HGDistortion.shader" },
            {"stubbedror2/base/shaders/speedtreecustom", "RoR2/Base/Shaders/SpeedTreeCustom.shader" },
            {"stubbedcalm water/calmwater - dx11 - doublesided", "Calm Water/CalmWater - DX11 - DoubleSided.shader" },
            {"stubbedcalm water/calmwater - dx11", "Calm Water/CalmWater - DX11.shader" },
            {"stubbednature/speedtree", "RoR2/Base/Shaders/SpeedTreeCustom.shader"}
        };

        internal static IEnumerator LoadAssetBundlesAsync(AssetBundle scenesAssetBundle, AssetBundle assetsAssetBundle, IProgress<float> progress, ContentPack contentPack)
        {
            _scenesAssetBundle = scenesAssetBundle;
            _assetsAssetBundle = assetsAssetBundle;
            //var expansionRequest = SnowtimeContent.LoadAssetAsync<ExpansionDef>("snowtimestageshalo_expdef", _assetsAssetBundle);
            //expansionRequest.StartLoad();
            //
            //while (!expansionRequest.IsComplete)
            //    yield return null;
            //
            //SnowtimeContent.expansionDefs.AddSingle(expansionRequest.Asset);
            Log.Debug($"Snowtime Stages found. Loading asset bundles...");

            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<Material[]>)((assets) =>
            {
                var materials = assets;

                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        if (!material.shader.name.StartsWith("Stubbed")) { continue; }

                        var replacementShader = Addressables.LoadAssetAsync<Shader>(ShaderLookup[material.shader.name.ToLower()]).WaitForCompletion();
                        if (replacementShader)
                        {
                            material.shader = replacementShader;
                            SwappedMaterials.Add(material);
                        }
                    }
                }
            }));

            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<UnlockableDef[]>)((assets) =>
            {
                UnlockableDefs = assets;
                contentPack.unlockableDefs.Add(assets);
            }));

            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<ExpansionDef[]>)((assets) =>
            {
                expansionDefs = assets;
                ExpansionDefSTHalo = assets.First(a => a.name == "snowtimestageshalo_expdef");
                Log.Debug("SnowtimeStages:Halo Expansion Definition Added");
                contentPack.expansionDefs.Add(assets);
            }));


            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<Sprite[]>)((assets) =>
            {
                STSceneDefPreviewSprite = assets.First(a => a.name == "texSTScenePreview");
                STIFSceneDefPreviewSprite = assets.First(a => a.name == "texSTIFScenePreview");
                STBGSceneDefPreviewSprite = assets.First(a => a.name == "texSTBGScenePreview");
                STGPHSceneDefPreviewSprite = assets.First(a => a.name == "texSTGPHScenePreview");
                STHSceneDefPreviewSprite = assets.First(a => a.name == "texSTHaloScenePreview");
                STH2SceneDefPreviewSprite = assets.First(a => a.name == "texSTHaloScenePreview");
                STShrineSceneDefPreviewSprite = assets.First(a => a.name == "texSTShrineScenePreview");
            }));

            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<SceneDef[]>)((assets) =>
            {
                SceneDefs = assets;
                STSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_deathisland");
                STIFSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_icefields");
                STBGSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_bloodgulch");
                STGPHSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_gephyrophobia");
                STHSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_halo");
                STH2SceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_halo2");
                STShrineSceneDef = SceneDefs.First(sd => sd.cachedName == "snowtime_sandtrap");
                Log.Debug(STSceneDef.nameToken);
                Log.Debug(STIFSceneDef.nameToken);
                Log.Debug(STBGSceneDef.nameToken);
                Log.Debug(STGPHSceneDef.nameToken);
                Log.Debug(STShrineSceneDef.nameToken);
                Log.Debug(STHSceneDef.nameToken);
                Log.Debug(STH2SceneDef.nameToken);
                contentPack.sceneDefs.Add(assets);
            }));

            yield return LoadAllAssetsAsync(_assetsAssetBundle, progress, (Action<MusicTrackDef[]>)((assets) =>
            {
                contentPack.musicTrackDefs.Add(assets);
                Log.Debug("loaded musicDefs for SnowtimeStages");
            }));

            // SetupMusic();

            //STBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STSceneDefPreviewSprite.texture);
            //STIFBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STIFSceneDefPreviewSprite.texture);
            //STBGBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STBGSceneDefPreviewSprite.texture);
            //STGPHBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STGPHSceneDefPreviewSprite.texture);
            //STHBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STHSceneDefPreviewSprite.texture);
            //STH2BazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STH2SceneDefPreviewSprite.texture);
            //STShrineBazaarSeer = StageRegistration.MakeBazaarSeerMaterial(STShrineSceneDefPreviewSprite.texture);
            STSceneDef.previewTexture = STSceneDefPreviewSprite.texture;
            STIFSceneDef.previewTexture = STIFSceneDefPreviewSprite.texture;
            STBGSceneDef.previewTexture = STBGSceneDefPreviewSprite.texture;
            STGPHSceneDef.previewTexture = STGPHSceneDefPreviewSprite.texture;
            STHSceneDef.previewTexture = STHSceneDefPreviewSprite.texture;
            STH2SceneDef.previewTexture = STH2SceneDefPreviewSprite.texture;
            STShrineSceneDef.previewTexture = STShrineSceneDefPreviewSprite.texture;
            //STSceneDef.portalMaterial = STBazaarSeer;
            //STIFSceneDef.portalMaterial = STIFBazaarSeer;
            //STBGSceneDef.portalMaterial = STBGBazaarSeer;
            //STGPHSceneDef.portalMaterial = STGPHBazaarSeer;
            //STHSceneDef.portalMaterial = STHBazaarSeer;
            //STH2SceneDef.portalMaterial = STH2BazaarSeer;
            //STShrineSceneDef.portalMaterial = STShrineBazaarSeer;

            StageRegistration.RegisterSceneDefToLoop(STSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STIFSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STBGSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STGPHSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STShrineSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STHSceneDef);
            StageRegistration.RegisterSceneDefToLoop(STH2SceneDef);
            Log.Debug(STSceneDef.destinationsGroup);
            Log.Debug(STIFSceneDef.destinationsGroup);
            Log.Debug(STBGSceneDef.destinationsGroup);
            Log.Debug(STGPHSceneDef.destinationsGroup);
            Log.Debug(STHSceneDef.destinationsGroup);
            Log.Debug(STH2SceneDef.destinationsGroup);
            Log.Debug(STShrineSceneDef.destinationsGroup);
        }

        private static IEnumerator LoadAllAssetsAsync<T>(AssetBundle assetBundle, IProgress<float> progress, Action<T[]> onAssetsLoaded) where T : UnityEngine.Object
        {
            var sceneDefsRequest = assetBundle.LoadAllAssetsAsync<T>();
            while (!sceneDefsRequest.isDone)
            {
                progress.Report(sceneDefsRequest.progress);
                yield return null;
            }

            onAssetsLoaded(sceneDefsRequest.allAssets.Cast<T>().ToArray());

            yield break;
        }

        /*internal static void LoadSoundBanks(string soundbanksFolderPath)
        {
            var akResult = AkSoundEngine.AddBasePath(soundbanksFolderPath);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank base path : {soundbanksFolderPath}");
            }
            else
            {
                Log.Error(
                    $"Error adding base path : {soundbanksFolderPath} " +
                    $"Error code : {akResult}");
            }

            akResult = AkSoundEngine.LoadBank(InitSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {InitSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {InitSoundBankFileName} " +
                    $"Error code : {akResult}");
            }

            akResult = AkSoundEngine.LoadBank(MusicSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {MusicSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {MusicSoundBankFileName} " +
                    $"Error code : {akResult}");
            }

            akResult = AkSoundEngine.LoadBank(SndSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {SndSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {SndSoundBankFileName} " +
                    $"Error code : {akResult}");
            }
        }*/
    }
}
