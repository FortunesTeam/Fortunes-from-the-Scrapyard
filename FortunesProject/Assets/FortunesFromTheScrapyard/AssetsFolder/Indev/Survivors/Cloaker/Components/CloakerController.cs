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
using RoR2.Skills;
using UnityEngine.Events;
using FortunesFromTheScrapyard.Items;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerController : MonoBehaviour
    {
        private CharacterBody characterBody;

        private ModelSkinController skinController;

        private ChildLocator childLocator;

        private CharacterModel characterModel;

        private Animator animator;

        private SkillLocator skillLocator;

        private CloakerRangeIndicatorComponent rangeIndicatorComponent;

        public static float baseRestealthCooldown = 7f;

        public static float baseGracePeriod = 3f;

        public bool isAkimbo => base.gameObject.GetComponent<CloakerPassive>().isAkimbo;
        private float restealthCooldown => Mathf.Min(baseRestealthCooldown, Mathf.Max(0.5f, baseRestealthCooldown * this.skillLocator.primary.cooldownScale - this.skillLocator.primary.flatCooldownReduction));

        private float restealthTimer;

        private GameObject nearbyIndicator;
        [HideInInspector]
        public float graceTimer = 0f;
        [HideInInspector]
        public bool passiveCloakOn = false;

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
                        if (characterBody.hasCloakBuff && passiveCloakOn) original = nearbyIndicator;

                        original.GetComponentInChildren<CloakerRangeIndicatorComponent>().ownerBody = characterBody;
                        original.GetComponentInChildren<CloakerRangeIndicatorComponent>().cloakerController = this;
                        original.GetComponentInChildren<CloakerRangeIndicatorComponent>()._teamIndex = characterBody.teamComponent.teamIndex;

                        if (original != null)
                        {
                            nearbyIndicator = UnityEngine.Object.Instantiate(original, characterBody.corePosition, Quaternion.identity);
                            nearbyIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                        }
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(nearbyIndicator);
                        nearbyIndicator = null;
                    }
                }
            }
        }
        private void Awake()    
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();

            indicatorEnabled = true;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            Invoke("PassiveSetup", 0.5f);
        }

        private void PassiveSetup()
        {
            if (isAkimbo)
            {
                skillLocator.primary.skillDef.cancelSprintingOnActivation = false;
                skillLocator.primary.skillDef.mustKeyPress = false;
            }
            else
            {
                skillLocator.primary.skillDef.cancelSprintingOnActivation = true;
                skillLocator.primary.skillDef.mustKeyPress = true;
            }
        }
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.hasCloakBuff && sender.TryGetComponent<CloakerController>(out var cloak) && cloak.passiveCloakOn)
            {
                args.moveSpeedMultAdd += 0.14f;
                args.damageMultAdd += 1.5f;
            }

            if(sender.TryGetComponent<CloakerController>(out var cloak2) && cloak2.isAkimbo)
            {
                args.attackSpeedMultAdd += 1f;
            }
        }

        private void Start()
        {
        }

        public void StartGracePeriod()
        {
            graceTimer = baseGracePeriod;
        }

        private void FixedUpdate()
        {
            if(isAkimbo)
            {
                passiveCloakOn = false;
                return;
            }

            if(graceTimer >= 0)
            {
                graceTimer -= Time.fixedDeltaTime;
            }

            if (characterBody.outOfCombat && !passiveCloakOn)
            {
                restealthTimer += Time.fixedDeltaTime;

                if (restealthTimer >= restealthCooldown && !characterBody.hasCloakBuff)
                {
                    restealthTimer = 0f;
                    passiveCloakOn = true;
                    characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                }
            }

            if (skillLocator.special.CanExecute() && skillLocator.special.skillDef.skillIndex == SkillCatalog.FindSkillIndexByName("Screech") && characterBody.hasCloakBuff && passiveCloakOn)
            {
                skillLocator.special.SetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("CloakerMark")), GenericSkill.SkillOverridePriority.Contextual);
            }
            else if (skillLocator.special.currentSkillOverride == SkillCatalog.FindSkillIndexByName("CloakerMark") && !characterBody.hasCloakBuff)
            {
                skillLocator.special.UnsetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("CloakerMark")), GenericSkill.SkillOverridePriority.Contextual);
            }
        }
        private void OnDestroy()
        {
            indicatorEnabled = false;

            UnityEngine.Object.Destroy(nearbyIndicator);
            nearbyIndicator = null;
        }
    }
}
