using UnityEngine;
using MSU;
using MSU.Config;
using RoR2.ContentManagement;
using RoR2;


namespace FortunesFromTheScrapyard.Characters.DukeDecoy
{
    public class DukeDecoy : ScrapyardCharacter
    {
        public static GameObject DukeDecoyMaster;
        public override void Initialize()
        {
            DukeDecoyMaster = assetCollection.FindAsset<GameObject>("DukeDecoyMaster");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<BodyAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<BodyAssetCollection>("acDukeDecoy", ScrapyardBundle.Indev);
        }
    }
}
