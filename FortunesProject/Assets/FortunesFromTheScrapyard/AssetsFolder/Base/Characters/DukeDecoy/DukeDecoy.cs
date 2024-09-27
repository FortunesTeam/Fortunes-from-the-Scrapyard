using UnityEngine;
using MSU;
using MSU.Config;
using RoR2.ContentManagement;
using RoR2;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using R2API;


namespace FortunesFromTheScrapyard.Characters.DukeDecoy
{
    public class DukeDecoy : ScrapyardCharacter
    {
        public static GameObject DukeDecoyMaster;
        public static DamageAPI.ModdedDamageType DecoyHit;
        public override void Initialize()
        {
            DecoyHit = DamageAPI.ReserveDamageType();
            DukeDecoyMaster = assetCollection.FindAsset<GameObject>("DukeDecoyMaster");

            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/Bandit2Crosshair");

            Hooks();
        }

        private void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitAllProcess += GlobalEventManager_OnHitAllProcess;
        }

        private void GlobalEventManager_OnHitAllProcess(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig.Invoke(self, damageInfo, hitObject);

            if(hitObject && damageInfo.attacker && !damageInfo.HasModdedDamageType(DecoyHit))
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
