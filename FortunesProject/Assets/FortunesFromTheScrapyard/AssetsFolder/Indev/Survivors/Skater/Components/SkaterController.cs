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
            On.RoR2.CharacterMotor.PreMove += CharacterMotor_PreMove;
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

        private void CharacterMotor_PreMove(On.RoR2.CharacterMotor.orig_PreMove orig, CharacterMotor self, float deltaTime)
        {
            if (self.body.bodyIndex != BodyCatalog.FindBodyIndex("SkaterBody"))
            {
                orig.Invoke(self, deltaTime);
                return;
            }
            if (!self.hasEffectiveAuthority)
            {
                return;
            }
            float num = self.acceleration;
            if (self.isAirControlForced || !self.isGrounded)
            {
                num *= (self.disableAirControlUntilCollision ? 0f : self.airControl);
            }
            Vector3 vector = self.moveDirection;
            if (!self.isFlying)
            {
                vector.y = 0f;
            }
            if (self.body.isSprinting)
            {
                float magnitude = vector.magnitude;
                if (magnitude < 1f && magnitude > 0f)
                {
                    float num2 = 1f / vector.magnitude;
                    vector *= num2;
                }
            }
            Vector3 target = vector * self.walkSpeed;
            if (!self.isFlying)
            {
                target.y = self.velocity.y;
            }
            self.velocity = Vector3.MoveTowards(self.velocity, target, num * deltaTime);
            if (self.useGravity)
            {
                ref float y = ref self.velocity.y;
                y += Physics.gravity.y * deltaTime;
                if (self.isGrounded)
                {
                    y = Mathf.Max(y, 0f);
                }
            }
        }
    }
}
