using EntityStates.Bandit2.Weapon;
using EntityStates.Mage.Weapon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.UI;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Cloaker;

namespace EntityStates.Cloaker.Weapon   
{
    public class CloakerChargeShot : BaseSkillState
    {
        public GameObject chargeEffectPrefab;

        public string chargeSoundString;

        public float baseDuration = 1.5f;

        public float minBloomRadius;

        public float maxBloomRadius;

        public GameObject crosshairOverridePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageCrosshair.prefab").WaitForCompletion();

        public float minChargeDuration = 0.5f;

        public CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public uint loopSoundInstanceId;

        public static int EmptyStateHash = Animator.StringToHash("Empty");

        public float duration { get; set; }
        public Animator animator { get; set; }
        public ChildLocator childLocator { get; set; }
        public GameObject chargeEffectInstance { get; set; }

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();

            if (NetworkServer.active)
            {
                this.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                this.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
                this.gameObject.GetComponent<CloakerController>().passiveCloakOn = false;
            }
            /*
            if (childLocator)
            {
                Transform transform = childLocator.FindChild("Muzzle") ?? base.characterBody.coreTransform;
                if ((bool)transform && (bool)chargeEffectPrefab)
                {
                    chargeEffectInstance = UnityEngine.Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                    chargeEffectInstance.transform.parent = transform;
                    ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    ObjectScaleCurve component2 = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                    if ((bool)component)
                    {
                        component.newDuration = duration;
                    }

                    if ((bool)component2)
                    {
                        component2.timeMax = duration;
                    }
                }
            }
            */

            PlayChargeAnimation();
            loopSoundInstanceId = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
            if (crosshairOverridePrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            }

            StartAimMode(duration + 2f);
        }

        public override void OnExit()
        {
            crosshairOverrideRequest?.Dispose();
            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (!outer.destroying)
            {
                PlayAnimation("Gesture, Additive", "BufferEmpty");
            }

            EntityState.Destroy(chargeEffectInstance);
            base.OnExit();
        }

        public float CalcCharge()
        {
            return Mathf.Clamp01(base.fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();
            if (base.isAuthority && ((!IsKeyDownAuthority() && base.fixedAge >= minChargeDuration) || base.fixedAge >= duration))
            {
                CloakerShoot shoot = new CloakerShoot
                {
                    baseDamageCoefficient = 2.6f * charge,
                    tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgun.prefab").WaitForCompletion(),
                    hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgun.prefab").WaitForCompletion(),
                    charged = true
                };

                outer.SetNextState(shoot);
            }
        }

        public override void Update()
        {
            base.Update();
            base.characterBody.SetSpreadBloom(Util.Remap(CalcCharge(), 0f, 1f, minBloomRadius, maxBloomRadius));
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public virtual void PlayChargeAnimation()
        {
            PlayAnimation("Gesture, Additive", "EnterSecondary", "Secondary.playbackRate", duration);
        }
    }
}