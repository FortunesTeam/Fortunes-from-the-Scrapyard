using UnityEngine;
using RoR2;
using EntityStates;
using BepInEx.Configuration;
// using BadgerMod.Modules;

namespace EntityStates.Badger
{
    public class MainState : GenericCharacterMain
    {
        private Animator animator;
        public LocalUser localUser;
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = this.modelAnimator;
            this.FindLocalUser();
        }
        private void FindLocalUser()
        {
            if (this.localUser == null)
            {
                if (base.characterBody)
                {
                    foreach (LocalUser lu in LocalUserManager.readOnlyLocalUsersList)
                    {
                        if (lu.cachedBody == base.characterBody)
                        {
                            this.localUser = lu;
                            break;
                        }
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.animator)
            {
                bool cock = false;
                if (!this.characterBody.outOfDanger || !this.characterBody.outOfCombat) cock = true;

                this.animator.SetBool("inCombat", cock);

                if (this.isGrounded) this.animator.SetFloat("airBlend", 0f);
                else this.animator.SetFloat("airBlend", 1f);
            }
        }

        public override void ProcessJump()
        {

            if (this.hasCharacterMotor)
            {
                bool hopooFeather = false;
                bool waxQuail = false;

                if (this.jumpInputReceived && base.characterBody && base.characterMotor.jumpCount < base.characterBody.maxJumpCount)
                {
                    int waxQuailCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
                    float horizontalBonus = 1f;
                    float verticalBonus = 1f;

                    if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
                    {
                        hopooFeather = true;
                        horizontalBonus = 1.5f;
                        verticalBonus = 1.5f;
                    }
                    else if (waxQuailCount > 0 && base.characterBody.isSprinting)
                    {
                        float v = base.characterBody.acceleration * base.characterMotor.airControl;

                        if (base.characterBody.moveSpeed > 0f && v > 0f)
                        {
                            waxQuail = true;
                            float num2 = Mathf.Sqrt(10f * (float)waxQuailCount / v);
                            float num3 = base.characterBody.moveSpeed / v;
                            horizontalBonus = (num2 + num3) / num3;
                        }
                    }

                    GenericCharacterMain.ApplyJumpVelocity(base.characterMotor, base.characterBody, horizontalBonus, verticalBonus, false);

                    if (this.hasModelAnimator)
                    {
                        int layerIndex = base.modelAnimator.GetLayerIndex("Body");
                        if (layerIndex >= 0)
                        {
                            if (this.characterBody.isSprinting)
                            {
                                this.modelAnimator.CrossFadeInFixedTime("SprintJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                            }
                            else
                            {
                                if (hopooFeather)
                                {
                                    this.modelAnimator.CrossFadeInFixedTime("BonusJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                                }
                                else
                                {
                                    this.modelAnimator.CrossFadeInFixedTime("Jump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                                }
                            }
                        }
                    }

                    if (hopooFeather)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                        {
                            origin = base.characterBody.footPosition
                        }, true);
                    }
                    else if (base.characterMotor.jumpCount > 0)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
                        {
                            origin = base.characterBody.footPosition,
                            scale = base.characterBody.radius
                        }, true);
                    }

                    if (waxQuail)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                        {
                            origin = base.characterBody.footPosition,
                            rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                        }, true);
                    }

                    base.characterMotor.jumpCount++;

                }
            }
        }
    }
}
