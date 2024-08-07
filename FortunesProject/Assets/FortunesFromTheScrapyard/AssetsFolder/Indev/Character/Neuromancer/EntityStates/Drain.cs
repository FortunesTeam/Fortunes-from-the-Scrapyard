using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections.ObjectModel;


using RoR2.Projectile;
using static UnityEngine.ParticleSystem.PlaybackState;
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using UnityEngine.UIElements;
using System.Linq;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class Drain : BaseNeuromancerSkillState
    {
        public static float duration = 0.25f;

        private GameObject neuromancerTimeStopField = Neuromancer.timeFreezeZoneStatic;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            if (neuromancerController.currentTimeEssence >= neuromancerController.maxTimeEssence)
            {
                drainController.DeactivateSiphon();

                neuromancerController.drainTimeEssence = true;

                if (base.isAuthority)
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

                    BlastAttack.Result result = new BlastAttack
                    {
                        attacker = base.gameObject,
                        procChainMask = default(ProcChainMask),
                        impactEffect = EffectIndex.Invalid,
                        losType = BlastAttack.LoSType.None,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        procCoefficient = 1f,
                        bonusForce = 400 * Vector3.up,
                        baseForce = 2000f,
                        baseDamage = 4f * this.damageStat,
                        falloffModel = BlastAttack.FalloffModel.None,
                        radius = 20f,
                        position = this.characterBody.corePosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        teamIndex = base.GetTeam(),
                        inflictor = base.gameObject,
                        crit = base.RollCrit()
                    }.Fire();

                    EffectManager.SpawnEffect(Neuromancer.kaboomEffect, new EffectData
                    {
                        origin = this.characterBody.corePosition,
                        rotation = Quaternion.identity,
                        scale = 1f
                    }, false);
                }
            }
            else if(!neuromancerController.drainTimeEssence)
            {
                if (drainController.siphonOn)
                {
                    drainController.DeactivateSiphon();
                }
                else
                {
                    drainController.ActivateSiphon();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= 0.25f)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}