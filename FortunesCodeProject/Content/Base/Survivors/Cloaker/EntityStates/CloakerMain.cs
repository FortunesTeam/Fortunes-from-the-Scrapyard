using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.Commando;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Cloaker.Components;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using RoR2.Skills;

namespace EntityStates.Cloaker
{
    public class CloakerMain : GenericCharacterMain 
    {
        public override void ProcessJump()
        {
            if (!hasCharacterMotor)
            {
                return;
            }
            bool flag = false;
            bool flag2 = false;
            bool flag3 = base.characterMotor.jumpCount < base.characterBody.maxJumpCount;
            if (!(jumpInputReceived && (bool)base.characterBody && flag3))
            {
                return;
            }
            int itemCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
            float horizontalBonus = 1f;
            float verticalBonus = 1f;
            if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
            {
                flag = true;
                horizontalBonus = 1.5f;
                verticalBonus = 1.5f;
            }
            else if ((float)itemCount > 0f && base.characterBody.isSprinting)
            {
                float num = base.characterBody.acceleration * base.characterMotor.airControl;
                if (base.characterBody.moveSpeed > 0f && num > 0f)
                {
                    flag2 = true;
                    float num2 = Mathf.Sqrt(10f * (float)itemCount / num);
                    float num3 = base.characterBody.moveSpeed / num;
                    horizontalBonus = (num2 + num3) / num3;
                }
            }
            ApplyJumpVelocity(base.characterMotor, base.characterBody, horizontalBonus, verticalBonus);
            if (hasModelAnimator)
            {
                int layerIndex = base.modelAnimator.GetLayerIndex("Body");
                string jump = modelAnimator.GetBool("isAkimbo") ? "Jump Dual" : "Jump";
                if (layerIndex >= 0)
                {
                    if (base.characterMotor.jumpCount == 0 || base.characterBody.baseJumpCount == 1)
                    {
                        base.modelAnimator.CrossFadeInFixedTime(jump, smoothingParameters.intoJumpTransitionTime, layerIndex);
                    }
                    else
                    {
                        base.modelAnimator.CrossFadeInFixedTime("BonusJump", smoothingParameters.intoJumpTransitionTime, layerIndex);
                    }
                }
            }
            if (flag)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                {
                    origin = base.characterBody.footPosition
                }, transmit: true);
            }
            else if (base.characterMotor.jumpCount > 0)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
                {
                    origin = base.characterBody.footPosition,
                    scale = base.characterBody.radius
                }, transmit: true);
            }
            if (flag2)
            {
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                {
                    origin = base.characterBody.footPosition,
                    rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                }, transmit: true);
            }
            base.characterMotor.jumpCount++;
            base.characterBody.onJump?.Invoke();
        }
    }
}