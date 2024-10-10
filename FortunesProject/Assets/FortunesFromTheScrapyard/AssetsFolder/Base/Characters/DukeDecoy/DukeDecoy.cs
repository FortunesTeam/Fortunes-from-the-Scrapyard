using UnityEngine;
using MSU;
using MSU.Config;
using RoR2.ContentManagement;
using RoR2;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;


namespace FortunesFromTheScrapyard.Characters.DukeDecoy
{
    public class DukeDecoy : ScrapyardCharacter
    {
        public static GameObject dukeDecoyDeathExplosion;
        public static GameObject DukeDecoyMaster;
        public static CharacterSpawnCard cscDukeDecoy;
        
        public override void Initialize()
        {
            DukeDecoyMaster = assetCollection.FindAsset<GameObject>("DukeDecoyMaster");
            cscDukeDecoy = assetCollection.FindAsset<CharacterSpawnCard>("cscDukeDecoy");

            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/Bandit2Crosshair");

            dukeDecoyDeathExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Child/ChildTrackingSparkBallExplosion.prefab").WaitForCompletion().InstantiateClone("DukeDecoyExplosion");
            if (!dukeDecoyDeathExplosion.GetComponent<NetworkIdentity>()) dukeDecoyDeathExplosion.AddComponent<NetworkIdentity>();

            ScrapyardContent.CreateAndAddEffectDef(dukeDecoyDeathExplosion);

            Hooks();
        }

        private void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitAllProcess += GlobalEventManager_OnHitAllProcess;
        }

        private void GlobalEventManager_OnHitAllProcess(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig.Invoke(self, damageInfo, hitObject);

            if(hitObject && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (hitObject.TryGetComponent<DukeDecoyExplosion>(out var boom))
                {
                    if(boom.ownerBody == attackerBody)
                    {
                        boom.SetValuesAndKillDecoy(damageInfo.damage / attackerBody.damage, damageInfo.crit);
                    }
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<BodyAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<BodyAssetCollection>("acDukeDecoy", ScrapyardBundle.Characters);
        }
    }
}
