using UnityEngine;
using MSU;
using MSU.Config;
using RoR2.ContentManagement;
using RoR2;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;


namespace FortunesFromTheScrapyard.Characters.DukeDecoy
{
    public class DukeDecoy : ScrapyardCharacter
    {
        public static GameObject DukeDecoyMaster;
        public override void Initialize()
        {
            DukeDecoyMaster = assetCollection.FindAsset<GameObject>("DukeDecoyMaster");

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
                        boom.damageCoefficient = damageInfo.damage / attackerBody.damage;

                        DamageInfo killDecoy = new DamageInfo();
                        killDecoy.attacker = null;
                        killDecoy.inflictor = null;
                        killDecoy.damage = boom.decoyBody.healthComponent.fullCombinedHealth;
                        killDecoy.procCoefficient = 0f;
                        killDecoy.crit = false;
                        killDecoy.damageType = DamageType.Silent;
                        killDecoy.damageColorIndex = DamageColorIndex.Default;
                        killDecoy.force = Vector3.zero;
                        killDecoy.position = boom.decoyBody.corePosition;

                        boom.decoyBody.healthComponent.TakeDamage(killDecoy);
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
