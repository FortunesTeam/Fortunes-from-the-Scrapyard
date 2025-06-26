using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;

namespace EntityStates.Skater
{
    public class TrickBaseState : BaseSkillState
    {
        protected virtual float baseDuration { get; set; } = 1f;
        protected virtual float trickEffectPercentTime { get; set; } = 0.4f;
        protected virtual string trickName { get; set; } = "Base";
        public static GameObject e = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/RolyPolyChargeStrongestExplosionVFX.prefab").WaitForCompletion();

        private float duration;
        private bool trickFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!trickFired && fixedAge >= duration * trickEffectPercentTime)
            {
                trickFired = true;
                Chat.AddMessage("Trick " + trickName + "!");
                Util.PlaySound("Play_chef_skill3_charge3", gameObject);
                EffectManager.SpawnEffect(e, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = 1
                }, transmit: true);
            }
            if ((fixedAge >= duration || characterMotor.isGrounded) && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (!trickFired && gameObject.TryGetComponent(out SetStateOnHurt hurt))
            {
                hurt.SetFrozen(2f);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge >= duration * trickEffectPercentTime)
            {
                return InterruptPriority.Skill;
            }
            return InterruptPriority.PrioritySkill;
        }

    }
}
