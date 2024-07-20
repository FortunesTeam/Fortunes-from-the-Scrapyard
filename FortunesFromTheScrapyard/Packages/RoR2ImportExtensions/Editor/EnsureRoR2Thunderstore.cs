using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using ThunderKit.Integrations.Thunderstore;
using UnityEditor;

namespace RiskOfThunder.RoR2Importer
{
    public class EnsureRoR2Thunderstore : OptionalExecutor
    {
        private const string RoR2ThunderstoreSourcePath = "Assets/ThunderKitSettings/RoR2Thunderstore.asset";

        public override int Priority => Constants.Priority.EnsureRoR2Thunderstore;
        public override string Description => $"Thunderstore related import options for RoR2";
        public override string Name => $"Ensure RoR2 Thunderstore Source";

        public override bool Execute()
        {
            var ror2Source = AssetDatabase.LoadAssetAtPath<ThunderstoreSource>(RoR2ThunderstoreSourcePath);
            if (ror2Source)
                return true;

            ror2Source = CreateInstance<ThunderstoreSource>();
            ror2Source.Url = "https://thunderstore.io";
            AssetDatabase.CreateAsset(ror2Source, RoR2ThunderstoreSourcePath);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ror2Source));
            PackageSource.LoadAllSources();
            return true;
        }
    }
}

