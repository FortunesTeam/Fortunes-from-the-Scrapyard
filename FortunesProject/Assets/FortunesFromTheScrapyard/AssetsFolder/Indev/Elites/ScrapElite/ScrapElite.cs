using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.RoR2Content.Equipment;
using static RoR2.DLC1Content.Equipment;
using static R2API.RecalculateStatsAPI;
using System;
using static RoR2.NetworkSession;
using System.Linq;

namespace FortunesFromTheScrapyard.Elite
{
    public sealed class ScrapElite : ScrapyardEliteEquipment
    {
        public const string TOKEN = "SCRAPYARD_EQUIP_SCRAP_ELITE_DESC";

        [ConfigureField(ScrapyardConfig.ID_ELITES)]
        public static bool enablePlayerLunars = false;

        public static List<EquipmentIndex> scrapEliteEquipmentListEnemy = new List<EquipmentIndex>();
        public static List<EquipmentIndex> scrapEliteEquipmentListPlayer = new List<EquipmentIndex>();
        public static GameObject ScrapAffixEffect;
        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
            ScrapAffixEffect = assetCollection.FindAsset<GameObject>("ScrapAffixEffect");

            On.RoR2.EquipmentCatalog.SetEquipmentDefs += EquipmentCatalog_SetEquipmentDefs;
        }
        private static void FillWhiteList(List<EquipmentIndex> filter)
        {
            foreach(EquipmentIndex index in filter) 
            { 
                if(!new[] { Recycle.equipmentIndex, MultiShopCard.equipmentIndex, BossHunter.equipmentIndex, BurnNearby.equipmentIndex }.Contains(index)) 
                {
                    if (!new[] { FireBallDash.equipmentIndex, Tonic.equipmentIndex }.Contains(index) && (!EquipmentCatalog.GetEquipmentDef(index).isLunar || enablePlayerLunars))
                    {
                        scrapEliteEquipmentListPlayer.Add(index);
                    }
                    if (!new[] { GoldGat.equipmentIndex, Lightning.equipmentIndex, CommandMissile.equipmentIndex, Saw.equipmentIndex, Blackhole.equipmentIndex, DroneBackup.equipmentIndex }.Contains(index))
                    {
                        scrapEliteEquipmentListEnemy.Add(index);
                    }
                }
            }

            scrapEliteEquipmentListEnemy.Add(QuestVolatileBattery.equipmentIndex);
            scrapEliteEquipmentListEnemy.Add(BossHunterConsumed.equipmentIndex);
        }
        private void EquipmentCatalog_SetEquipmentDefs(On.RoR2.EquipmentCatalog.orig_SetEquipmentDefs orig, EquipmentDef[] newEquipmentDefs)
        {
            scrapEliteEquipmentListPlayer.Clear();
            scrapEliteEquipmentListEnemy.Clear();

            orig(newEquipmentDefs);

            FillWhiteList(EquipmentCatalog.enigmaEquipmentList);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest<EliteAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<EliteAssetCollection>("acScrapElite", ScrapyardBundle.Indev);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            if (NetworkServer.active && body.HasBuff(ScrapyardContent.Buffs.bdEliteScrap)) body.RemoveBuff(ScrapyardContent.Buffs.bdEliteScrap);

        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            if (NetworkServer.active) body.AddBuff(ScrapyardContent.Buffs.bdEliteScrap);
        }

        public class ScrapEliteBehaviour : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => ScrapyardContent.Buffs.bdEliteScrap;

            public PickupDisplay pickupDisplay;


            private float baseEquipmentCDFaked;
            private float equipmentCDTimer;
            private EquipmentIndex chosenEquipmentIndex;
            private GameObject effectInstance;
            private TimerHologramContent timerDisplay;
            private void Start()
            {
                List<EquipmentIndex> list = CharacterBody.isPlayerControlled ? new List<EquipmentIndex>(scrapEliteEquipmentListPlayer) : new List<EquipmentIndex>(scrapEliteEquipmentListEnemy);
                Util.ShuffleList(list);
                chosenEquipmentIndex = list[list.Count - 1];
                baseEquipmentCDFaked = EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex).cooldown / 2;

                if(!effectInstance)
                {
                    effectInstance = UnityEngine.Object.Instantiate(ScrapAffixEffect, CharacterBody.modelLocator.modelBaseTransform);

                    if (!CharacterBody.isPlayerControlled)
                    {
                        effectInstance.transform.Find("EquipmentPrefab").localScale *= 2f;
                    }

                    pickupDisplay = effectInstance.transform.Find("EquipmentPrefab").gameObject.GetComponent<PickupDisplay>();
                }

                if (pickupDisplay)
                {
                    pickupDisplay.SetPickupIndex(PickupCatalog.FindPickupIndex(chosenEquipmentIndex));

                    if ((bool)pickupDisplay.modelRenderer)
                    {
                        Highlight component = GetComponent<Highlight>();
                        if ((bool)component)
                        {
                            component.targetRenderer = pickupDisplay.modelRenderer;
                        }
                    }
                }
                ScrapyardLog.Debug("Current Equipment:" + EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex).name);
            }

            private void OnDisable()
            {
                if(effectInstance) UnityEngine.Object.Destroy(effectInstance);
                effectInstance = null;
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }

                if (scrapEliteEquipmentListEnemy.Count <= 0 || scrapEliteEquipmentListPlayer.Count <= 0 && chosenEquipmentIndex > 0)
                {
                    return;
                }

                equipmentCDTimer += Time.fixedDeltaTime;

                if (equipmentCDTimer >= baseEquipmentCDFaked) 
                {
                    equipmentCDTimer = 0;

                    timerDisplay.displayValue = baseEquipmentCDFaked;

                    CharacterBody.equipmentSlot.PerformEquipmentAction(EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex));

                    if (chosenEquipmentIndex == RoR2Content.Equipment.BFG.equipmentIndex)
                    {
                        ModelLocator component = GetComponent<ModelLocator>();
                        if ((bool)component)
                        {
                            Transform modelTransform = component.modelTransform;
                            if ((bool)modelTransform)
                            {
                                CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
                                if ((bool)component2)
                                {
                                    List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.RandomEquipmentTrigger.itemIndex);
                                    if (itemDisplayObjects.Count > 0)
                                    {
                                        UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/ChargeBFG.prefab").WaitForCompletion(), itemDisplayObjects[0].transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}