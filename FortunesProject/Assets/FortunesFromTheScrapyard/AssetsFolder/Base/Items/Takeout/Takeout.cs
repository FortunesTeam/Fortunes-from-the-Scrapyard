using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Items
{
    public class Takeout : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_TAKEOUT_DESC";
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float igniteBase = 2.5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float igniteStack = 2.5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 2)]
        public static int chickenCooldown = 5;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float mspdBase = 0.45f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float mspdStack = 0.45f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 5)]
        public static float regenBase = 1.5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 6)]
        public static float regenStack = 1.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 7)]
        public static float healBase = 0.075f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 8)]
        public static float healStack = 0.075f;

        private static WeightedSelection<BuffDef> _weightedBuffSelection = new WeightedSelection<BuffDef>();

        public static GameObject noodlesRadiusEffect;
        public static GameObject potstickersRadiusEffect;
        public static GameObject chickenRadiusEffect;
        public static GameObject potstickerImpactEffect;
        public static GameObject chickenExplosionEffect;
        public override void Initialize()
        {
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutDmg"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutSpeed"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutRegen"), 10);

            noodlesRadiusEffect = CreateTakeoutEffect("NoodlesRangeIndicator", new Color(0.07450981f, 0.6431373f, 0.5238169f, 0.5019608f));

            potstickersRadiusEffect = CreateTakeoutEffect("PotstickersRangeIndicator", new Color(0.09803922f, 0.6431373f, 0.07450981f, 0.5019608f));

            chickenRadiusEffect = CreateTakeoutEffect("ChickenRangeIndicator", new Color(0.6431373f, 0.08564561f, 0.07450981f, 0.5019608f));

            potstickerImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleAcidImpact.prefab").WaitForCompletion().InstantiateClone("PotstickersImpactEffect", false);
            potstickerImpactEffect.EnsureComponent<EffectComponent>();

            EffectDef potStickerImpactDef = new EffectDef(potstickerImpactEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(potStickerImpactDef);

            chickenExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/WilloWispExplosion.prefab").WaitForCompletion().InstantiateClone("ChickenExplosionEffect", false);
            chickenExplosionEffect.EnsureComponent<EffectComponent>();

            EffectDef chickenDef = new EffectDef(chickenExplosionEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(chickenDef);
        }

        private GameObject CreateTakeoutEffect(string prefabName, Color color)
        {
            GameObject foodf = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion();

            GameObject food = AssetCollection.FindAsset<GameObject>(prefabName);
            food.EnsureComponent<NetworkIdentity>();

            food.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material = foodf.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material;
            food.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);

            food.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material = foodf.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material;
            food.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);

            return food;
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acTakeout", ScrapyardBundle.Items);
        }
        public class TakeoutBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = false)]
            private static ItemDef GetItemDef() => ScrapyardContent.Items.Takeout;

            private GameObject nearbyIndicator;

            private bool indicatorEnabled
            {
                get
                {
                    return nearbyIndicator;
                }
                set
                {
                    if (indicatorEnabled != value)
                    {
                        if (value)
                        {
                            GameObject original = null;
                            if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutDmg)) original = chickenRadiusEffect;
                            else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutSpeed)) original = noodlesRadiusEffect;
                            else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutRegen)) original = potstickersRadiusEffect;

                            original.GetComponentInChildren<TakeoutComponent>().ownerBody = base.body;
                            original.GetComponentInChildren<TakeoutComponent>()._teamIndex = base.body.teamComponent.teamIndex;

                            if (original != null)
                            {
                                nearbyIndicator = Object.Instantiate(original, base.body.corePosition, Quaternion.identity);
                                nearbyIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                            }
                        }
                        else
                        {
                            Object.Destroy(nearbyIndicator);
                            nearbyIndicator = null;
                        }
                    }
                }
            }

            private void OnEnable()
            {
                if (!NetworkServer.active)
                    return;

                var buffDef = _weightedBuffSelection.Evaluate(Random.value);
                body.AddBuff(buffDef);

                indicatorEnabled = true;
            }

            private void OnDisable()
            {
                if (NetworkServer.active)
                {
                    if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutDmg))
                    {
                        if (body.HasBuff(ScrapyardContent.Buffs.bdChicken)) body.RemoveBuff(ScrapyardContent.Buffs.bdChicken);
                        if (body.HasBuff(ScrapyardContent.Buffs.bdChickenCooldown)) body.RemoveBuff(ScrapyardContent.Buffs.bdChickenCooldown);
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutDmg);
                    }
                    else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutSpeed))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutSpeed);
                    }
                    else if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutRegen))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdTakeoutRegen);
                    }
                }
                indicatorEnabled = false;
            }

            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(ScrapyardContent.Buffs.bdPotstickers))
                {
                    args.baseRegenAdd += GetStackValue(regenBase, regenStack, stack);
                }
                if (body.HasBuff(ScrapyardContent.Buffs.bdNoodles))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, stack);
                }
            }
        }
    }
}
