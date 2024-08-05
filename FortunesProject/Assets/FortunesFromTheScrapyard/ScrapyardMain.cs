using BepInEx;
using MSU;
using Mono.Cecil;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using RoR2.Artifacts;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning disable CS0618
[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[module: UnverifiableCode]

namespace FortunesFromTheScrapyard
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.Moffein.RiskyArtifacts", BepInDependency.DependencyFlags.SoftDependency)]
    public class ScrapyardMain : BaseUnityPlugin
    {
        public const string GUID = "com.FortunesTeam.FortunesFromTheScrapyard";
        public const string VERSION = "0.0.1";
        public const string NAME = "Fortunes From the Scrapyard";

        public static bool RiskyArtifactsLoaded = false;

        //Singleton access pattern to our instance.
        internal static ScrapyardMain instance { get; private set; }

        private void Awake()
        {
            instance = this;

            RiskyArtifactsLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.RiskyArtifacts");

            new ScrapyardLog(Logger);
            new ScrapyardConfig(this);

            //We do not load our assetbundles or content at awake, instead, we create a new instance of this class,
            //which implements the game's IContentPackProvider interface.
            new ScrapyardContent();

            LanguageFileLoader.AddLanguageFilesFromMod(this, "ScrapyardLanguage");
        }

        private void Start()
        {
            ScrapyardSoundbank.Init();
        }
        public static float GetProjectileSimpleModifiers(float speed)
        {
            if (RiskyArtifactsLoaded) speed *= GetRiskyArtifactsWarfareProjectileSpeedMult();
            return speed;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetRiskyArtifactsWarfareProjectileSpeedMult()
        {
            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(Risky_Artifacts.Artifacts.Warfare.artifact))
            {
                return Risky_Artifacts.Artifacts.Warfare.projSpeed;
            }
            return 1f;
        }
    }
}