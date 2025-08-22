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
        
        private float restealthCooldown;

        private float restealthTimer;

        private GameObject nearbyIndicator;
        [HideInInspector]
        public float graceTimer = 0f;
        [HideInInspector]
        public bool passiveCloakOn = false;

        public SkillDef passiveCloakSkillDef;

        public SkillDef passiveAkimboSkillDef;

        public GenericSkill passiveSkillSlot;
        [HideInInspector]
        public bool isCloak;
        [HideInInspector]
        public bool isAkimbo;

        private bool wasOutOfCombat;

        private void EvaluatePassiveSkill()
        {
            isCloak = false;
            isAkimbo = false;
            if (passiveSkillSlot)
            {
                if (passiveCloakSkillDef)
                {
                    isCloak = passiveSkillSlot.skillDef == passiveCloakSkillDef;
                }
                if (passiveAkimboSkillDef)
                {
                    isAkimbo = passiveSkillSlot.skillDef == passiveAkimboSkillDef;
                }
            }
            indicatorEnabled = isCloak;
            if (isAkimbo)
            {
                SetAkimboActive(true);
                skillLocator.primary.skillDef.cancelSprintingOnActivation = false;
                skillLocator.primary.skillDef.mustKeyPress = false;
            }
            else
            {
                SetAkimboActive(false); 
                skillLocator.primary.skillDef.cancelSprintingOnActivation = true;
                skillLocator.primary.skillDef.mustKeyPress = true;
            }
        }

        private void SetAkimboActive(bool value)
        {
            if (animator)
            {
                animator.SetBool("isAkimbo", value);
                animator.SetFloat("isAkimboFloat", value ? 1 : 0);
            }
            if (childLocator)
            {
                childLocator.FindChild("DualGunMesh").gameObject.SetActive(value);
            }
        }

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
                        if (!nearbyIndicator)
                        {
                            nearbyIndicator = UnityEngine.Object.Instantiate(Cloaker.CloakerRangeIndicatorPrefab, characterBody.corePosition, Quaternion.identity);
                            if (nearbyIndicator)
                            {
                                CloakerRangeIndicatorComponent cloakerRangeIndicator = nearbyIndicator.transform.Find("ProximityTrigger").gameObject.GetComponent<CloakerRangeIndicatorComponent>();
                                cloakerRangeIndicator.ownerBody = characterBody;
                                cloakerRangeIndicator._teamIndex = characterBody.teamComponent.teamIndex;
                                cloakerRangeIndicator.cloakerController = this;
                                nearbyIndicator.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
                            }
                        }
                    }
                    if (nearbyIndicator)
                    {
                        nearbyIndicator.SetActive(value);
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
        }

        private void OnEnable()
        {
            if (passiveSkillSlot)
            {
                passiveSkillSlot.onSkillChanged += ReevaluatePassiveOnChange;
            }
            Invoke("EvaluatePassiveSkill", Time.fixedDeltaTime);
            SetStealthCooldown();
            StartGracePeriod();
        }

        private void ReevaluatePassiveOnChange(GenericSkill skill)
        {
            EvaluatePassiveSkill();
        }

        public void SetStealthCooldown()
        {
            restealthCooldown = Mathf.Min(baseRestealthCooldown, Mathf.Max(0.5f, baseRestealthCooldown * this.skillLocator.primary.cooldownScale - this.skillLocator.primary.flatCooldownReduction));
        }
        public void StartGracePeriod()
        {
            graceTimer = baseGracePeriod;
        }
        public void ActivateCloak()
        {
            if (passiveCloakOn)
            { 
                return; 
            }
            SetStealthCooldown();
            restealthTimer = restealthCooldown;
            passiveCloakOn = true;
            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
            }
            if (characterBody)
            {
                characterBody.onSkillActivatedAuthority += CharacterBody_onSkillActivatedAuthority;
            }
        }

        private void CharacterBody_onSkillActivatedAuthority(GenericSkill skill)
        {
            if (skill.skillDef.isCombatSkill)
            {
                DeactivateCloak();
            }
        }

        public void DeactivateCloak()
        {
            if (!passiveCloakOn)
            {
                return;
            }
            SetStealthCooldown();
            restealthTimer = restealthCooldown;
            passiveCloakOn = true;
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
            }
            if (characterBody)
            {
                characterBody.onSkillActivatedAuthority -= CharacterBody_onSkillActivatedAuthority;
            }
        }

        private void FixedUpdate()
        {
            if (isAkimbo)
            {
                return;
            }

            /*if (graceTimer > 0f)
            {
                graceTimer -= Time.fixedDeltaTime;
                if (characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
                {
                    return;
                }
            }*/

            if (characterBody.outOfCombat)
            {
                if (!wasOutOfCombat)
                {
                    SetStealthCooldown();
                }
                restealthTimer -= Time.fixedDeltaTime;

                if (restealthTimer <= 0f && !characterBody.hasCloakBuff && !passiveCloakOn)
                {
                    ActivateCloak();
                }
            }
            else if (passiveCloakOn)
            {
                DeactivateCloak();
            }
            wasOutOfCombat = characterBody.outOfCombat;
        }
        private void OnDisable()
        {
            if (passiveSkillSlot)
            {
                passiveSkillSlot.onSkillChanged -= ReevaluatePassiveOnChange;
            }
            indicatorEnabled = false;

            UnityEngine.Object.Destroy(nearbyIndicator);
            nearbyIndicator = null;
        }
    }
}
