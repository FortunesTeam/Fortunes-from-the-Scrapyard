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

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class NeuromancerController : MonoBehaviour
    {

        private CharacterBody characterBody;

        private ModelSkinController skinController;

        private ChildLocator childLocator;

        private CharacterModel characterModel;

        private Animator animator;

        private SkillLocator skillLocator;

        public static float startingMaxTimeStop = 2.5f;
        public static float baseMaxOverheat = 3.5f;

        public DamageAPI.ModdedDamageType ModdedDamageType;

        public float maxTimeEssence = 1.5f;

        public float currentTimeEssence = 0f;

        public Action onEssenceChange;

        public bool drainTimeEssence = false;

        public float maxOverheat = 3.5f;

        public float overheatStopwatch = 0f;
        public bool drainOverheat;
        public bool hitMaxOverheat;

        private uint timePlayId;
        private uint playID2;

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
        private void Start()
        {
            if(skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("TimeStop")))
            {
                this.maxTimeEssence = startingMaxTimeStop + ((this.skillLocator.special.flatCooldownReduction + startingMaxTimeStop) * (1 - this.skillLocator.special.cooldownScale));
            }
            else if(skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SiphonTime")))
            {
                this.maxTimeEssence = 2f * (startingMaxTimeStop + ((this.skillLocator.special.flatCooldownReduction + startingMaxTimeStop) * (1 - this.skillLocator.special.cooldownScale)));
            }

            if(skillLocator.primary.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Beam")))
            {
                this.maxOverheat = baseMaxOverheat + ((this.skillLocator.primary.flatCooldownReduction + baseMaxOverheat) * (1 - this.skillLocator.primary.cooldownScale));
            }
            else
            {
                this.maxOverheat = 0f;
            }
        }

        private void FixedUpdate()
        {
            if(overheatStopwatch >= 0f && drainOverheat)
            {
                overheatStopwatch -= Time.deltaTime;
            }
            else if(overheatStopwatch <= 0f && drainOverheat)
            {
                hitMaxOverheat = false;
                drainOverheat = false;
                overheatStopwatch = 0f;
            }

            if (drainTimeEssence)
            {
                currentTimeEssence -= Time.fixedDeltaTime;

                if (currentTimeEssence <= 0f)
                {
                    DeactivateTimeField();
                }

                this.onEssenceChange?.Invoke();
            }
            else
            {
                if(skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("TimeStop")))
                {
                    if (currentTimeEssence < maxTimeEssence)
                    {
                        currentTimeEssence += Time.fixedDeltaTime / 4f;
                        if (currentTimeEssence > maxTimeEssence) currentTimeEssence = maxTimeEssence;
                        this.onEssenceChange?.Invoke();
                    }
                    else if (currentTimeEssence > maxTimeEssence)
                    {
                        currentTimeEssence -= Time.fixedDeltaTime / 8f;
                        if (currentTimeEssence < maxTimeEssence) currentTimeEssence = maxTimeEssence;
                        this.onEssenceChange?.Invoke();
                    }
                }
                else if(skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SiphonTime")))
                {
                    if (currentTimeEssence > maxTimeEssence)
                    {
                        currentTimeEssence -= Time.fixedDeltaTime / 8f;
                        if (currentTimeEssence < maxTimeEssence) currentTimeEssence = maxTimeEssence;
                        this.onEssenceChange?.Invoke();
                    }
                }
            }

            if (skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("TimeStop")))
            {
                this.maxTimeEssence = startingMaxTimeStop + ((this.skillLocator.special.flatCooldownReduction + startingMaxTimeStop) * (1 - this.skillLocator.special.cooldownScale));
            }
            else if (skillLocator.special.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("SiphonTime")))
            {
                this.maxTimeEssence = 2f * (startingMaxTimeStop + ((this.skillLocator.special.flatCooldownReduction + startingMaxTimeStop) * (1 - this.skillLocator.special.cooldownScale)));
            }

            if (skillLocator.primary.skillDef == SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Beam")))
            {
                this.maxOverheat = baseMaxOverheat + ((this.skillLocator.primary.flatCooldownReduction + baseMaxOverheat) * (1 - this.skillLocator.primary.cooldownScale));
            }
            else
            {
                this.maxOverheat = 0f;
            }
        }

        public void SapTimeEssence(float time, bool canOverCap = false)
        {
            if(canOverCap)
            {
                currentTimeEssence += time;
            }
            else
            {
                float num = maxTimeEssence - currentTimeEssence;

                if (num < 0f) num = 0f;
                
                currentTimeEssence += Mathf.Clamp(time, 0f, num);
            }
        }

        public void ActivateTimeField()
        {
            drainTimeEssence = true;
        }
        public void DeactivateTimeField()
        {
            drainTimeEssence = false;
        }        
    }
}
