using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.Skills;
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    public class DukeController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        public float attackSpeedConversion;
        public bool speedUpReloadTime;
        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = characterBody.skillLocator;
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
        }

        private void Start()
        {
            characterBody.RecalculateStats();
        }
    }
}
