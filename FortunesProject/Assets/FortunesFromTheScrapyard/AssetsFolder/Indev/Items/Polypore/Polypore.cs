using JetBrains.Annotations;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace FortunesFromTheScrapyard.Items
{
    public class Polypore : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_POLYPORE_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePolyporeDamage = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float basePolyporeDamageStack = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 2)]
        public static float basePolyporeTimer = 3f;

        //[ConfigureField(ScrapyardConfig.ID_ITEMS)]
        //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        //public static float basePolyporeTimerStack = 0.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float basePolyporePopRadius = 8f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float basePolyporePopRadiusStack = 2.8f;

        public static DamageAPI.ModdedDamageType PeeDamage;

        public override void Initialize()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            PeeDamage = DamageAPI.ReserveDamageType();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig.Invoke(self, damageInfo, victim);
            if (!NetworkServer.active) { return; }

            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (victimBody && attackerBody && attackerBody.HasItem(ScrapyardContent.Items.Polypore))
            {
                if (!victimBody.HasBuff(ScrapyardContent.Buffs.bdPolypore))
                {
                    bool hasDebuff = false;
                    BuffIndex[] debuffBuffIndices = BuffCatalog.debuffBuffIndices;
                    foreach (BuffIndex buffType in debuffBuffIndices)
                    {
                        if (victimBody.HasBuff(buffType))
                        {
                            hasDebuff = true;
                            break;
                        }
                    }
                    DotController dotController = DotController.FindDotController(victimBody.gameObject);
                    if ((bool)dotController)
                    {
                        for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                        {
                            if (dotController.HasDotActive(dotIndex))
                            {
                                hasDebuff = true;
                                break;
                            }
                        }
                    }
                    if (hasDebuff == true)
                    {
                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdPolypore, basePolyporeTimer);
                        FartComponent victimFart = victimBody.gameObject.EnsureComponent<FartComponent>();
                        victimFart.fartCount = attackerBody.GetItemCount(ScrapyardContent.Items.Polypore);
                        victimFart.fartObject = damageInfo.attacker;

                    }
                }
            }
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acPolypore", ScrapyardBundle.Indev);
        }
        public class PolyporeBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.Polypore;
        }

        public class FartComponent : MonoBehaviour
        {
            public int fartCount = 0;
            public GameObject fartObject;
        }

        public class PeeComponent : MonoBehaviour
        {
            List<BuffIndex> fartHolder = new List<BuffIndex>();
        }

        public class PolyporeBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => ScrapyardContent.Buffs.bdPolypore;

            private void OnEnable()
            {

            }

            private void OnDisable()
            {
                int farts = base.gameObject.GetComponent<FartComponent>().fartCount;
                GameObject fartObject = base.gameObject.GetComponent<FartComponent>().fartObject;
                List<BuffIndex> activeDebuffs = new List<BuffIndex>();
                BuffIndex[] debuffBuffIndices = BuffCatalog.debuffBuffIndices;
                foreach (BuffIndex buffType in debuffBuffIndices)
                {
                    if (characterBody.HasBuff(buffType))
                    {
                        activeDebuffs.Add(buffType);
                    }
                }

                float dotTotalDamage = 0f;

                DotController dotController = DotController.FindDotController(characterBody.gameObject);
                if ((bool)dotController)
                {
                    for (DotController.DotIndex dotIndex = DotController.DotIndex.Bleed; dotIndex < DotController.DotIndex.Count; dotIndex++)
                    {
                        if (dotController.HasDotActive(dotIndex))
                        {
                            DotController.DotDef dotDef = DotController.GetDotDef(dotIndex);
                            for (int num = dotController.dotStackList.Count - 1; num >= 0; num--)
                            {
                                DotController.DotStack dotStack = dotController.dotStackList[num];
                                if (dotStack.dotIndex == dotIndex)
                                {
                                    dotTotalDamage += dotStack.damage;
                                }
                            }
                        }
                    }
                }

                BlastAttack blastAttack = new BlastAttack();

                blastAttack.procCoefficient = 0f;
                blastAttack.attacker = fartObject;
                blastAttack.inflictor = null;
                blastAttack.teamIndex = fartObject.GetComponent<CharacterBody>().teamComponent.teamIndex;
                blastAttack.baseDamage = (fartObject.GetComponent<CharacterBody>().damage * dotTotalDamage * basePolyporeDamage) - ((fartObject.GetComponent<CharacterBody>().damage * dotTotalDamage * basePolyporeDamage) * (basePolyporeDamageStack * farts- 1));
                blastAttack.baseForce = 100f;
                blastAttack.position = characterBody.corePosition;
                blastAttack.radius = GetStackValue(basePolyporePopRadius, basePolyporePopRadiusStack, farts);
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.damageColorIndex = DamageColorIndex.DeathMark;
                blastAttack.AddModdedDamageType(PeeDamage);

                Util.CleanseBody(characterBody, true,false,false,true,true,false);

                blastAttack.Fire();

                EffectManager.SpawnEffect(Headphones.headphonesShockwavePrefab, new EffectData
                {
                    origin = characterBody.corePosition,
                    rotation = Quaternion.identity,
                    scale = 1f
                }, true);

                Component.Destroy(characterBody.gameObject.GetComponent<FartComponent>());
            }

            private void FixedUpdate()
            {

            }

        }
    }
}
