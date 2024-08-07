using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.Projectile;
using On.EntityStates.VoidJailer.Weapon;
using IL.RoR2.Items;
using System.Collections.Generic;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class NeuromancerDrainController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private NeuromancerController neuromancerController;
        private ModelLocator modelLocator;

        [HideInInspector]
        public bool siphonOn = false;

        public float maxTetherDistance = 13f;

        public float damagePerSecond = 2f;

        public float damageTickFrequency = 0.3f;

        public GameObject areaIndicator = Neuromancer.nearbySiphonIndicator;

        public string enterSoundString;

        public string beginMulchSoundString;

        public string stopMulchSoundString;

        private GameObject nearbyEffect;

        private float stopwatch;

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            this.modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
            this.neuromancerController = this.GetComponent <NeuromancerController>();

        }
        private void Start()
        {
        }

        private void FixedUpdate()
        {
             if (siphonOn)
            {
                if (NetworkServer.active)
                {
                    stopwatch -= Time.fixedDeltaTime;

                    if(stopwatch <= 0f)
                    {
                        stopwatch += damageTickFrequency;

                        HurtBox[] hurtBoxes = new SphereSearch
                        {
                            origin = characterBody.corePosition,
                            radius = 13f,
                            mask = LayerIndex.entityPrecise.mask
                        }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(characterBody.teamComponent.teamIndex)).OrderCandidatesByDistance()
                            .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                        foreach (HurtBox hurtBox in hurtBoxes)
                        {
                            GameObject gameObject = hurtBox.healthComponent.gameObject;
                            CharacterBody body = hurtBox.healthComponent.body;
                            if (gameObject && body)
                            {
                                float damageCoefficientPerTick = damagePerSecond / (damageTickFrequency * 10f);
                                DamageInfo damageInfo = new DamageInfo
                                {
                                    position = body.corePosition,
                                    attacker = base.gameObject,
                                    inflictor = null,
                                    damage = damageCoefficientPerTick * characterBody.damage,
                                    damageColorIndex = DamageColorIndex.Default,
                                    damageType = DamageType.Generic,
                                    crit = characterBody.RollCrit(),
                                    force = Vector3.zero,
                                    procChainMask = default(ProcChainMask),
                                    procCoefficient = 0.3f
                                };
                                damageInfo.AddModdedDamageType(Neuromancer.AltSpecialSiphon);
                                hurtBox.healthComponent.TakeDamage(damageInfo);
                            }
                        }
                    }
                }
            }
        }

        public void ActivateSiphon()
        {
            siphonOn = true;

            Util.PlaySound(beginMulchSoundString, base.gameObject);

            nearbyEffect = UnityEngine.Object.Instantiate(areaIndicator, characterBody.corePosition, Quaternion.identity);
            nearbyEffect.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
            ChildLocator component2 = nearbyEffect.gameObject.GetComponent<ChildLocator>();
            if (component2)
            {
                Transform transform = component2.FindChild("Chest");
                if (transform)
                {
                    transform.localScale = new Vector3(maxTetherDistance * 2f, maxTetherDistance * 2f, maxTetherDistance * 2f);
                }
            }
        }

        public void DeactivateSiphon()
        {
            siphonOn = false;

            if (nearbyEffect)
            {
                UnityEngine.Object.Destroy(nearbyEffect);
                nearbyEffect = null;
            }

            // AkSoundEngine.StopPlayingID(soundID);
            // Util.PlaySound(stopMulchSoundString, base.gameObject);
        }
        private void OnDestroy()
        {
        }
    }
}
