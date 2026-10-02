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
    public class Takeout : FFTSItem
    {
        public const string TOKEN = "FFTS_ITEM_TAKEOUT_DESCRIPTION";
        internal const float radius = 13f;
        internal const int noodlesDecaySteps = 10;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float burnBase = 1f;
        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float burnStack = 1f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float mspdBase = 0.07f;
        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float mspdStack = 0.07f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 5)]
        public static float regenBase = 1.5f;
        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 6)]
        public static float regenStack = 1.5f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        public static float buffDuration = 7.5f;

        // Preserve the existing description's placeholders until its text is updated.
        [FormatToken(TOKEN, 2)]
        public static int chickenCooldown = 0;
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 7)]
        public static float healBase = 0f;
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 8)]
        public static float healStack = 0f;

        private static readonly WeightedSelection<BuffDef> _weightedBuffSelection = new WeightedSelection<BuffDef>();

        internal static readonly Color noodlesColor = new Color(0.07450981f, 0.6431373f, 0.5238169f, 0.5019608f);
        internal static readonly Color potstickersColor = new Color(0.09803922f, 0.6431373f, 0.07450981f, 0.5019608f);
        internal static readonly Color chickenColor = new Color(0.6431373f, 0.08564561f, 0.07450981f, 0.5019608f);

        public static GameObject noodlesRadiusEffect;
        public static GameObject potstickersRadiusEffect;
        public static GameObject chickenRadiusEffect;
        public static GameObject potstickerImpactEffect;
        private static NetworkSoundEventDef activationSound;

        public override void Initialize()
        {
            activationSound = FFTSContent.CreateAndAddNetworkSoundEventDef("sfx_energybar_use");
            _weightedBuffSelection.AddChoice(assetCollection.FindAsset<BuffDef>("bdTakeoutDmg"), 10);
            _weightedBuffSelection.AddChoice(assetCollection.FindAsset<BuffDef>("bdTakeoutSpeed"), 10);
            _weightedBuffSelection.AddChoice(assetCollection.FindAsset<BuffDef>("bdTakeoutRegen"), 10);

            assetCollection.FindAsset<BuffDef>("bdNoodles").canStack = true;

            noodlesRadiusEffect = CreateTakeoutEffect("NoodlesRangeIndicator", noodlesColor);

            potstickersRadiusEffect = CreateTakeoutEffect("PotstickersRangeIndicator", potstickersColor);

            chickenRadiusEffect = CreateTakeoutEffect("ChickenRangeIndicator", chickenColor);

            AssetAsyncReferenceManager<GameObject>.LoadAsset(new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_BeetleQueen.BeetleAcidImpact_prefab)).Completed += x =>
            {
                potstickerImpactEffect = x.Result.InstantiateClone("PotstickersImpactEffect", false);
                potstickerImpactEffect.EnsureComponent<EffectComponent>();

                FFTSContent.CreateAndAddEffectDef(potstickerImpactEffect);
            };
        }

        private GameObject CreateTakeoutEffect(string prefabName, Color color)
        {
            GameObject foodf = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab").WaitForCompletion();

            GameObject food = assetCollection.FindAsset<GameObject>(prefabName);
            food.EnsureComponent<NetworkIdentity>();

            food.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material = foodf.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material;
            food.transform.Find("Donut").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);

            food.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material = foodf.transform.Find("Radius, Spherical").gameObject.GetComponent<MeshRenderer>().material;
            food.transform.Find("Radius").gameObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);
            food.transform.Find("Radius").localScale = Vector3.one * radius;

            food.GetComponentInChildren<Collider>().enabled = false;

            return food;
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acTakeout", FFTSBundle.Items);
        }
        public class TakeoutBehaviour : BaseItemBodyBehavior, IBodyStatArgModifier, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => FFTSContent.Items.Takeout;

            private GameObject nearbyIndicator;
            private BuffDef nextFood;

            private void OnEnable()
            {
                if (!NetworkServer.active)
                    return;

                RollNextFood();
                nearbyIndicator = Object.Instantiate(noodlesRadiusEffect, body.corePosition, Quaternion.identity);
                nearbyIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
            }

            private void OnDisable()
            {
                if (NetworkServer.active)
                {
                    if (nextFood)
                        body.RemoveBuff(nextFood);
                    nextFood = null;
                    body.ClearTimedBuffs(FFTSContent.Buffs.bdNoodles);
                    body.ClearTimedBuffs(FFTSContent.Buffs.bdPotstickers);
                }
                Object.Destroy(nearbyIndicator);
                nearbyIndicator = null;
            }

            private void RollNextFood()
            {
                if (nextFood)
                    body.RemoveBuff(nextFood);

                nextFood = _weightedBuffSelection.Evaluate(Random.value);
                body.AddBuff(nextFood);
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (!NetworkServer.active || !isActiveAndEnabled || stack <= 0 || !nextFood)
                    return;

                DamageInfo damageInfo = damageReport.damageInfo;
                if (damageInfo.rejected || damageInfo.procCoefficient <= 0f || damageInfo.damage <= 0f
                    || damageInfo.dotIndex != DotController.DotIndex.None || !damageReport.victimBody
                    || !TeamMask.GetEnemyTeams(body.teamComponent.teamIndex).HasTeam(damageReport.victimTeamIndex)
                    || (body.corePosition - damageInfo.position).sqrMagnitude > radius * radius)
                    return;

                if (nextFood == FFTSContent.Buffs.bdTakeoutDmg)
                {
                    float burnMultiplier = GetStackValue(burnBase, burnStack, stack);
                    InflictDotInfo dotInfo = new InflictDotInfo
                    {
                        attackerObject = body.gameObject,
                        victimObject = damageReport.victim.gameObject,
                        dotIndex = DotController.DotIndex.Burn,
                        totalDamage = damageInfo.damage * 0.5f * burnMultiplier,
                        damageMultiplier = burnMultiplier
                    };
                    StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                    DotController.InflictDot(ref dotInfo);
                }
                else if (nextFood == FFTSContent.Buffs.bdTakeoutSpeed)
                {
                    body.ClearTimedBuffs(FFTSContent.Buffs.bdNoodles);
                    for (int i = 1; i <= noodlesDecaySteps; i++)
                    {
                        body.AddTimedBuff(FFTSContent.Buffs.bdNoodles, buffDuration * i / noodlesDecaySteps);
                    }
                }
                else if (nextFood == FFTSContent.Buffs.bdTakeoutRegen)
                {
                    body.AddTimedBuff(FFTSContent.Buffs.bdPotstickers, buffDuration);
                    EffectManager.SpawnEffect(potstickerImpactEffect, new EffectData
                    {
                        origin = body.corePosition
                    }, transmit: true);
                }

                RollNextFood();
                EffectManager.SimpleSoundEffect(activationSound.index, body.corePosition, true);
            }

            public void ModifyStatArguments(StatHookEventArgs args)
            {
                if (body.HasBuff(FFTSContent.Buffs.bdPotstickers))
                {
                    args.baseRegenAdd += GetStackValue(regenBase, regenStack, stack);
                }
                if (body.HasBuff(FFTSContent.Buffs.bdNoodles))
                {
                    args.moveSpeedMultAdd += GetStackValue(mspdBase, mspdStack, stack)
                        * body.GetBuffCount(FFTSContent.Buffs.bdNoodles) / noodlesDecaySteps;
                }
            }
        }
    }
}
