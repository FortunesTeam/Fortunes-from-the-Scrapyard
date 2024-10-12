using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Skater.Components
{
    public class SkaterController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterMotor characterMotor;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        
        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            characterMotor = GetComponent<CharacterMotor>();
            skillLocator = GetComponent<SkillLocator>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            animator = modelLocator.modelTransform.GetComponent<Animator>();
            skinController = this.GetComponentInChildren<ModelSkinController>();
            Hooks();
        }

        private void Start()
        {
            characterMotor.airControl = 0.75f;
        }
        private void Hooks()
        {
            On.RoR2.CharacterMotor.Start += CharacterMotor_Start;
        }

        private void CharacterMotor_Start(On.RoR2.CharacterMotor.orig_Start orig, CharacterMotor self)
        {
            orig.Invoke(self);
            if (self.body.bodyIndex == BodyCatalog.FindBodyIndex("SkaterBody"))
            {
                self.Motor.MaxStableSlopeAngle = 45f;
            }
        }
    }
}
