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
    public class Polypore : FFTSItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_POLYPORE_DESC";

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float basePolyporeDamage = 0.5f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float basePolyporeDamageStack = 0.5f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 2)]
        public static float basePolyporeTimer = 3f;

        //[ConfigureField(FFTSConfig.ID_ITEMS)]
        //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        //public static float basePolyporeTimerStack = 0.5f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static float basePolyporePopRadius = 8f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static float basePolyporePopRadiusStack = 2.8f;

        public static DamageAPI.ModdedDamageType PolyporeDamage;

        public override void Initialize()
        {
            //On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            //PolyporeDamage = DamageAPI.ReserveDamageType();
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig.Invoke(self, damageInfo, victim);
            if (!NetworkServer.active || damageInfo.rejected) { return; }

            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (victimBody && attackerBody && attackerBody.HasItem(FFTSContent.Items.Polypore))
            {
                if (!victimBody.HasBuff(FFTSContent.Buffs.bdPolypore))
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
                        victimBody.AddTimedBuff(FFTSContent.Buffs.bdPolypore, basePolyporeTimer);
                        PolyporeComponent polyporeComponent = victimBody.gameObject.EnsureComponent<PolyporeComponent>();
                        polyporeComponent.polyporeCount = attackerBody.GetItemCount(FFTSContent.Items.Polypore);
                        polyporeComponent.polyporeObject = damageInfo.attacker;

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

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acPolypore", FFTSBundle.Indev);
        }
        public class PolyporeBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => FFTSContent.Items.Polypore;
        }

        public class PolyporeComponent : MonoBehaviour
        {
            public int polyporeCount = 0;
            public GameObject polyporeObject;
        }

        public class OtherComponent : MonoBehaviour
        {
            List<BuffIndex> buffHolder = new List<BuffIndex>();
        }

        public class PolyporeBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => FFTSContent.Buffs.bdPolypore;

            private void OnEnable()
            {

            }

            private void OnDisable()
            {
                int polyporeCount = base.gameObject.GetComponent<PolyporeComponent>().polyporeCount;
                GameObject polyporeObject = base.gameObject.GetComponent<PolyporeComponent>().polyporeObject;
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
                blastAttack.attacker = polyporeObject;
                blastAttack.inflictor = null;
                blastAttack.teamIndex = polyporeObject.GetComponent<CharacterBody>().teamComponent.teamIndex;
                blastAttack.baseDamage = (polyporeObject.GetComponent<CharacterBody>().damage * dotTotalDamage * basePolyporeDamage) - ((polyporeObject.GetComponent<CharacterBody>().damage * dotTotalDamage * basePolyporeDamage) * (basePolyporeDamageStack * polyporeCount - 1));
                blastAttack.baseForce = 100f;
                blastAttack.position = characterBody.corePosition;
                blastAttack.radius = GetStackValue(basePolyporePopRadius, basePolyporePopRadiusStack, polyporeCount);
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.damageColorIndex = DamageColorIndex.DeathMark;
                blastAttack.AddModdedDamageType(PolyporeDamage);

                Util.CleanseBody(characterBody, true, false, false, true, true, false);

                blastAttack.Fire();

                EffectManager.SpawnEffect(BrokenHeadphones.headphonesShockwavePrefab, new EffectData
                {
                    origin = characterBody.corePosition,
                    rotation = Quaternion.identity,
                    scale = 1f
                }, true);

                Component.Destroy(characterBody.gameObject.GetComponent<PolyporeComponent>());
            }

            private void FixedUpdate()
            {

            }

        }
    }
}
