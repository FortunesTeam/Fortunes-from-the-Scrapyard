using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using RoR2.Projectile;
using MSU;
using static UnityEngine.UI.GridLayoutGroup;

namespace FortunesFromTheScrapyard.Items
{
    public class LethalInjection : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_LETHALINJECTION_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float toxinPercentBase = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float toxinPercentStack = 0.5f;

        public static GameObject lethalInjectionPrefab;
        public override void Initialize()
        {
            lethalInjectionPrefab = assetCollection.FindAsset<GameObject>("LethalInjectionPrefab");

            On.RoR2.HealthComponent.GetHealthBarValues += HealthComponent_GetHealthBarValues;
        }

        private HealthComponent.HealthBarValues HealthComponent_GetHealthBarValues(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            var bar = orig(self);
            if (!self.body.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToExecutes) && self.body.HasBuff(ScrapyardContent.Buffs.bdLethalInjection) && self.TryGetComponent<InjectionBehavior>(out var component))
            {
                bar.cullFraction += component.injectionExecuteThreshold;
            }
            return bar;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acLethalInjection", ScrapyardBundle.Items);
        }

        public class LethalInjectionBehaviour : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.LethalInjection;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                CharacterBody attackerBody = body;
                CharacterBody victimBody = damageReport.victimBody;
                HealthComponent victimHealth = damageReport.victimBody.healthComponent;
                if (victimBody && attackerBody && damageInfo.damage / attackerBody.damage >= 4 && damageInfo.procCoefficient > 0)
                {
                    if (NetworkServer.active)
                    {
                        if (NetworkServer.active) victimBody.AddBuff(ScrapyardContent.Buffs.bdLethalInjection);
                        float injectionExecuteDamagePercentage = (damageInfo.damage / victimHealth.fullCombinedHealth) * (GetStackValue(toxinPercentBase, toxinPercentStack, stack) * damageInfo.procCoefficient);
                        InjectionBehavior injection = victimBody.gameObject.EnsureComponent<InjectionBehavior>();
                        if(injection.hostBody != victimBody) injection.hostBody = victimBody;
                        injection.AddStacks(injectionExecuteDamagePercentage);
                        Vector3 position = damageInfo.position;
                        Vector3 forward = victimBody.corePosition - position;
                        float magnitude = forward.magnitude;
                        Quaternion rotation = Util.QuaternionSafeLookRotation(forward);
                        GameObject injectionProjectile = LethalInjection.lethalInjectionPrefab;
                        ProjectileManager.instance.FireProjectile(injectionProjectile, position, rotation, damageInfo.attacker, 0f, 100f, false, DamageColorIndex.Default, null, victimBody.healthComponent.alive ? (magnitude * 5f) : (-1f));
                    }
                }

                if(victimBody && victimHealth && victimBody.HasBuff(ScrapyardContent.Buffs.bdLethalInjection))
                {
                    float executionHealthLost = 0f;
                    GameObject executeEffect = null;
                    float executeThreshold = Mathf.NegativeInfinity;
                    float executeHealthThreshold = victimBody.gameObject.EnsureComponent<InjectionBehavior>().injectionExecuteThreshold;
                    if (executeThreshold < executeHealthThreshold)
                    {
                        executeThreshold = executeHealthThreshold;
                        executeEffect = RoR2.HealthComponent.AssetReferences.permanentDebuffEffectPrefab;
                    }

                    if(executeThreshold > 0f && victimHealth.combinedHealthFraction <= executeThreshold) 
                    {
                        executionHealthLost = Mathf.Max(victimHealth.combinedHealth, 0f);
                        if (victimHealth.health > 0f)
                        {
                            victimHealth.Networkhealth = 0f;
                        }
                        if (victimHealth.shield > 0f)
                        {
                            victimHealth.Networkshield = 0f;
                        }
                        if (victimHealth.barrier > 0f)
                        {
                            victimHealth.Networkbarrier = 0f;
                        }
                    }

                    if(!victimHealth.alive)
                    {
                        GlobalEventManager.ServerCharacterExecuted(damageReport, executionHealthLost);
                        if (executeEffect != null)
                        {
                            EffectManager.SpawnEffect(executeEffect, new EffectData
                            {
                                origin = victimBody.corePosition,
                                scale = (victimBody ? victimBody.radius : 1f)
                            }, transmit: true);
                        }
                    }
                }
            }
        }

        public class InjectionBehavior : MonoBehaviour
        {
            internal CharacterBody hostBody;
            public float injectionExecuteThreshold;
            public void Start()
            {
                if (hostBody == null) hostBody = GetComponent<CharacterBody>();
            }
            public void AddStacks(float percentageToAdd)
            {
                hostBody.statsDirty = true;
                injectionExecuteThreshold += Util.ConvertAmplificationPercentageIntoReductionPercentage(percentageToAdd);
            }
        }
    }
}
