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
using R2API;
using RoR2.Projectile;

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
        public static GameObject ScrapPulseEffect;
        public static GameObject ScrapExplosionEffect;
        public static GameObject ScrapDefenseMatrix;
        public static GameObject ScrapLaser;
        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
            ScrapAffixEffect = assetCollection.FindAsset<GameObject>("ScrapAffixEffect");

            ScrapPulseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMine.prefab").WaitForCompletion().gameObject.transform.Find("PrepEffect").gameObject.InstantiateClone("ScrapPulseEffect");
            ScrapPulseEffect.EnsureComponent<NetworkIdentity>();
            ScrapPulseEffect.gameObject.SetActive(true);
            EffectComponent effectComponent = ScrapPulseEffect.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            ScrapPulseEffect.AddComponent<DestroyOnParticleEnd>();
            VFXAttributes vfx = ScrapPulseEffect.AddComponent<VFXAttributes>();
            vfx.vfxPriority = VFXAttributes.VFXPriority.Medium;
            vfx.vfxIntensity = VFXAttributes.VFXIntensity.Low;

            EffectDef effectDef = new EffectDef(ScrapPulseEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef);

            ScrapExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineExplosion.prefab").WaitForCompletion().InstantiateClone("ScrapExplosionEffect", false);

            EffectDef effectDef2 = new EffectDef(ScrapExplosionEffect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef2);

            ScrapDefenseMatrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CaptainDefenseMatrix/CaptainDefenseMatrixItemBodyAttachment.prefab").WaitForCompletion().InstantiateClone("ScrapMatrix");
            ScrapDefenseMatrix.EnsureComponent<NetworkIdentity>();

            UnityEngine.Object.Destroy(ScrapDefenseMatrix.transform.Find("Spinner").Find("Mesh").gameObject);
            UnityEngine.Object.Destroy(ScrapDefenseMatrix.transform.Find("Spinner").gameObject);

            ScrapLaser = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CaptainDefenseMatrix/TracerCaptainDefenseMatrix.prefab").WaitForCompletion().InstantiateClone("ScrapLaser");
            ScrapLaser.EnsureComponent<EffectComponent>();

            EffectDef effectDef3 = new EffectDef(ScrapLaser);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef3);

            On.RoR2.EquipmentCatalog.SetEquipmentDefs += EquipmentCatalog_SetEquipmentDefs;
        }
        private static void FillWhiteList(List<EquipmentIndex> filter)
        {
            foreach (EquipmentIndex index in filter)
            {
                if (!new[] { Recycle.equipmentIndex, MultiShopCard.equipmentIndex, BossHunter.equipmentIndex, BurnNearby.equipmentIndex }.Contains(index))
                {
                    if (!new[] { FireBallDash.equipmentIndex, Tonic.equipmentIndex }.Contains(index) && (!EquipmentCatalog.GetEquipmentDef(index).isLunar || enablePlayerLunars))
                    {
                        scrapEliteEquipmentListPlayer.Add(index);
                    }
                    if (!new[] { GoldGat.equipmentIndex, Lightning.equipmentIndex, CommandMissile.equipmentIndex, BFG.equipmentIndex, Saw.equipmentIndex, Blackhole.equipmentIndex, DroneBackup.equipmentIndex }.Contains(index))
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
            return ScrapyardAssets.LoadAssetAsync<EliteAssetCollection>("acScrapElite", ScrapyardBundle.Elites);
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

            private static float playerTimerCycle = 5f;

            private float baseEquipmentCDFaked;
            private float equipmentCDTimer;
            private EquipmentIndex chosenEquipmentIndex;
            private GameObject affixEffectGameObject;
            private float playerTimer;

            private static float basePulseCooldown = 10f;
            private float pulseTimer = 0f;
            private float chargeUpTimer = 0f;
            private bool charging = false;

            //private NetworkedBodyAttachment displayAttachment;

            private NetworkedBodyAttachment batteryAttachment;

            #region captain shenegnangings
            private GameObject matrixAttachmentGameObject;

            private NetworkedBodyAttachment matrixAttachment;

            public static float projectileEraserRadius = 10f;

            public static float baseRechargeFrequency = 5f;

            public static GameObject tracerEffectPrefab = ScrapLaser;

            private float rechargeTimer;

            private Xoroshiro128Plus rng;
            private bool matrixAttachmentActive
            {
                get
                {
                    return (object)matrixAttachment != null;
                }
                set
                {
                    if (value != matrixAttachmentActive)
                    {
                        if (value)
                        {
                            matrixAttachmentGameObject = UnityEngine.Object.Instantiate(ScrapDefenseMatrix);
                            matrixAttachment = matrixAttachmentGameObject.GetComponent<NetworkedBodyAttachment>();
                            matrixAttachment.AttachToGameObjectAndSpawn(characterBody.gameObject);
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(matrixAttachmentGameObject);
                            matrixAttachmentGameObject = null;
                            matrixAttachment = null;
                        }
                    }
                }
            }
            #endregion

            private void Start()
            {
            }
            private void OnEnable()
            {
                rng = new Xoroshiro128Plus(Run.instance.seed ^ (ulong)Run.instance.stageClearCount);

                Util.PlaySound("sfx_scrap_elite_spawn", base.gameObject);

                playerTimer = 0f;

                List<EquipmentIndex> list = characterBody.isPlayerControlled ? new List<EquipmentIndex>(scrapEliteEquipmentListPlayer) : new List<EquipmentIndex>(scrapEliteEquipmentListEnemy);
                Util.ShuffleList(list, rng);
                chosenEquipmentIndex = list[list.Count - 1];
                baseEquipmentCDFaked = EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex).cooldown / 2f;
                equipmentCDTimer = baseEquipmentCDFaked / 2f;

                if (!affixEffectGameObject)
                {
                    affixEffectGameObject = UnityEngine.Object.Instantiate(ScrapAffixEffect, characterBody.transform);

                    pickupDisplay = affixEffectGameObject.transform.Find("ScrapAffixEquipment").gameObject.GetComponent<PickupDisplay>();

                    if (pickupDisplay)
                    {
                        pickupDisplay.SetPickupIndex(PickupCatalog.FindPickupIndex(chosenEquipmentIndex));

                        if (pickupDisplay.modelRenderer)
                        {
                            Highlight component = affixEffectGameObject.transform.Find("ScrapAffixEquipment").gameObject.GetComponent<Highlight>();
                            if (component)
                            {
                                component.targetRenderer = pickupDisplay.modelRenderer;
                            }
                        }
                    }
                    //displayAttachment = affixEffectGameObject.GetComponent<NetworkedBodyAttachment>();
                    //displayAttachment.AttachToGameObjectAndSpawn(CharacterBody.gameObject);
                }

                if (chosenEquipmentIndex == RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex)
                {
                    batteryAttachment = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/QuestVolatileBatteryAttachment")).GetComponent<NetworkedBodyAttachment>();
                    batteryAttachment.AttachToGameObjectAndSpawn(characterBody.gameObject);
                }
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();

                if (batteryAttachment)
                {
                    UnityEngine.Object.Destroy(batteryAttachment.gameObject);
                    batteryAttachment = null;
                }
            }
            private void OnDisable()
            {
                if (affixEffectGameObject)
                {
                    UnityEngine.Object.Destroy(affixEffectGameObject);
                    affixEffectGameObject = null;
                    //displayAttachment = null;
                }

                matrixAttachmentActive = false;

                if (matrixAttachment)
                {
                    UnityEngine.Object.Destroy(matrixAttachmentGameObject);
                    matrixAttachmentGameObject = null;
                    matrixAttachment = null;
                }
            }

            private void FixedUpdate()
            {
                matrixAttachmentActive = characterBody.healthComponent.alive;

                if (scrapEliteEquipmentListEnemy.Count <= 0 || scrapEliteEquipmentListPlayer.Count <= 0 && chosenEquipmentIndex > 0)
                {
                    return;
                }

                equipmentCDTimer += Time.fixedDeltaTime;
                playerTimer += Time.fixedDeltaTime;
                rechargeTimer -= Time.fixedDeltaTime;

                //Matrix Behaviour
                if (rechargeTimer <= 0f)
                {
                    if (DeleteNearbyProjectile())
                    {
                        rechargeTimer = baseRechargeFrequency;
                    }
                }

                //Pushback Behaviour
                if (!charging)
                {
                    if (pulseTimer <= basePulseCooldown) pulseTimer += Time.fixedDeltaTime;
                    else if (pulseTimer >= basePulseCooldown)
                    {
                        HurtBox[] hurtBoxes = new SphereSearch
                        {
                            origin = characterBody.corePosition,
                            radius = 10f,
                            mask = LayerIndex.entityPrecise.mask
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(characterBody.teamComponent.teamIndex)).OrderCandidatesByDistance()
                        .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        if (hurtBoxes.Length > 0)
                        {

                            EffectManager.SpawnEffect(ScrapPulseEffect, new EffectData
                            {
                                origin = base.transform.position,
                            }, transmit: true);

                            charging = true;
                            pulseTimer = 0f;
                        }
                    }
                }
                else
                {
                    chargeUpTimer += Time.fixedDeltaTime;

                    if (chargeUpTimer >= 1f)
                    {
                        charging = false;
                        chargeUpTimer = 0f;

                        BlastAttack blastAttack = new BlastAttack
                        {
                            attacker = base.gameObject,
                            procChainMask = default(ProcChainMask),
                            losType = BlastAttack.LoSType.None,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.FallDamage | DamageType.BypassBlock | DamageType.WeakOnHit,
                            procCoefficient = 0f,
                            bonusForce = new Vector3(0f, 200f, 0f),
                            baseForce = 4000f,
                            baseDamage = characterBody.damage * 0.5f,
                            falloffModel = BlastAttack.FalloffModel.None,
                            radius = 20f,
                            position = base.transform.position,
                            attackerFiltering = AttackerFiltering.NeverHitSelf,
                            teamIndex = characterBody.teamComponent.teamIndex,
                            inflictor = base.gameObject,
                            crit = characterBody.isPlayerControlled ? characterBody.RollCrit() : false
                        };

                        blastAttack.Fire();

                        EffectManager.SpawnEffect(ScrapExplosionEffect, new EffectData
                        {
                            origin = base.transform.position,
                        }, transmit: true);
                    }
                }

                //Player only hide massive prefab
                if (characterBody.isPlayerControlled && playerTimer >= playerTimerCycle)
                {
                    pickupDisplay.gameObject.SetActive(false);
                }

                //Fire equipment
                if (equipmentCDTimer >= baseEquipmentCDFaked && !characterBody.outOfCombat)
                {
                    equipmentCDTimer = 0f;
                    playerTimer = 0f;

                    if (characterBody.isPlayerControlled) pickupDisplay.gameObject.SetActive(true);

                    if (!characterBody.equipmentSlot)
                    {
                        characterBody.equipmentSlot = characterBody.gameObject.EnsureComponent<EquipmentSlot>();
                        characterBody.equipmentSlot.characterBody = characterBody;
                        characterBody.equipmentSlot.healthComponent = characterBody.healthComponent;
                        characterBody.equipmentSlot.teamComponent = characterBody.teamComponent;
                        characterBody.equipmentSlot.targetIndicator = new Indicator(base.gameObject, null);
                    }

                    characterBody.equipmentSlot.PerformEquipmentAction(EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex));

                    if (chosenEquipmentIndex == RoR2Content.Equipment.BFG.equipmentIndex)
                    {
                        ModelLocator component = GetComponent<ModelLocator>();
                        if (component)
                        {
                            Transform modelTransform = component.modelTransform;
                            if (modelTransform)
                            {
                                CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
                                if (component2)
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
            private bool DeleteNearbyProjectile()
            {
                Vector3 vector = (characterBody ? characterBody.corePosition : Vector3.zero);
                TeamIndex teamIndex = (characterBody ? characterBody.teamComponent.teamIndex : TeamIndex.None);
                float num = projectileEraserRadius * projectileEraserRadius;
                int num2 = 0;
                bool result = false;
                List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
                List<ProjectileController> list = new List<ProjectileController>();
                int i = 0;
                for (int count = instancesList.Count; i < count; i++)
                {
                    if (num2 >= 1)
                    {
                        break;
                    }
                    ProjectileController projectileController = instancesList[i];
                    if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < num)
                    {
                        list.Add(projectileController);
                        num2++;
                    }
                }
                int j = 0;
                for (int count2 = list.Count; j < count2; j++)
                {
                    ProjectileController projectileController2 = list[j];
                    if (projectileController2)
                    {
                        result = true;
                        Vector3 position = projectileController2.transform.position;
                        Vector3 start = vector;
                        if (tracerEffectPrefab)
                        {
                            EffectData effectData = new EffectData
                            {
                                origin = position,
                                start = start
                            };
                            EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                        }
                        UnityEngine.Object.Destroy(projectileController2.gameObject);
                    }
                }
                return result;
            }
        }
    }
}