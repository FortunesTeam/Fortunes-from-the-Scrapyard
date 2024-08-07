using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections.ObjectModel;


using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class TimeStop : BaseNeuromancerSkillState
    {
        private GameObject neuromancerTimeStopField = Neuromancer.timeFreezeZone;
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            if(neuromancerController.drainTimeEssence == true)
            {
                neuromancerController.DeactivateTimeField();
            }
            else
            {
                neuromancerController.ActivateTimeField();

                if(base.isAuthority)
                {
                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = neuromancerTimeStopField;
                    fireProjectileInfo.position = characterBody.corePosition;
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(characterMotor.Motor.CharacterForward);
                    fireProjectileInfo.owner = base.gameObject;
                    fireProjectileInfo.damage = 1f;
                    fireProjectileInfo.force = 0f;
                    fireProjectileInfo.crit = false;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge >= 0.25f)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}