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

namespace FortunesFromTheScrapyard.Items
{
    public class Takeout : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_TAKEOUT_DESC";
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageBase = 0.25f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageStack = 0.25f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float mspdBase = 0.25f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float mspdStack = 0.25f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float regenBase = 1.25f;
        [FormatToken(TOKEN, 5)]
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float regenStack = 1.25f;

        private static WeightedSelection<BuffDef> _weightedBuffSelection = new WeightedSelection<BuffDef>();

        public static GameObject noodlesRadiusEffect;
        public static GameObject potstickersRadiusEffect;
        public override void Initialize()
        {
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutDmg"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutSpeed"), 10);
            _weightedBuffSelection.AddChoice(AssetCollection.FindAsset<BuffDef>("bdTakeoutRegen"), 10);

            GameObject noodlesRadiusEffectf = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion().InstantiateClone("NoodlesRadiusEffect");
            GameObject potstickersRadiusEffectf = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion().InstantiateClone("PotstickersRadiusEffect");

            noodlesRadiusEffect = AssetCollection.FindAsset<GameObject>("NoodlesRangeIndicator");
            noodlesRadiusEffect.EnsureComponent<NetworkIdentity>();

            noodlesRadiusEffect.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material = noodlesRadiusEffectf.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material;
            noodlesRadiusEffect.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.07450981f, 0.6431373f, 0.5238169f, 0.5019608f));

            noodlesRadiusEffect.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material = noodlesRadiusEffectf.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material;
            noodlesRadiusEffect.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.07450981f, 0.6431373f, 0.5238169f, 0.5019608f));

            potstickersRadiusEffect = AssetCollection.FindAsset<GameObject>("PotstickersRangeIndicator");
            potstickersRadiusEffect.EnsureComponent<NetworkIdentity>();

            potstickersRadiusEffect.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material = potstickersRadiusEffectf.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material;
            potstickersRadiusEffect.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.09803922f, 0.6431373f, 0.07450981f, 0.5019608f));

            potstickersRadiusEffect.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material = potstickersRadiusEffectf.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material;
            potstickersRadiusEffect.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", new Color(0.09803922f, 0.6431373f, 0.07450981f, 0.5019608f));

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

            private string currentEffect;

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
                            if (currentEffect == "Potstickers") original = potstickersRadiusEffect;
                            else if (currentEffect == "Noodles") original = noodlesRadiusEffect;

                            original.GetComponentInChildren<RangeIndicator>().ownerBody = base.body;
                            original.GetComponentInChildren<RangeIndicator>()._teamIndex = base.body.teamComponent.teamIndex;

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

                if(buffDef == ScrapyardContent.Buffs.bdTakeoutDmg) { return; }
                else if(buffDef == ScrapyardContent.Buffs.bdTakeoutRegen) { currentEffect = "Potstickers"; }
                else if(buffDef == ScrapyardContent.Buffs.bdTakeoutSpeed) { currentEffect = "Noodles"; }

                indicatorEnabled = true;
            }

            private void OnDisable()
            {
                if (NetworkServer.active)
                {
                    if (body.HasBuff(ScrapyardContent.Buffs.bdTakeoutDmg))
                    {
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
                    args.regenMultAdd += GetStackValue(regenBase, regenStack, stack);
                }
                if (body.HasBuff(ScrapyardContent.Buffs.bdNoodles))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, stack);
                }
            }
        }
    }
}
