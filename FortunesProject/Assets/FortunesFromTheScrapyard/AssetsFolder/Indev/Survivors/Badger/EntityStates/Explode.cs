using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
// using BadgerMod.Badger.Content;
using EntityStates.Badger.Components;
using R2API;
// using BadgerMod.Modules.BaseStates;
using KinematicCharacterController;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Badger;

namespace EntityStates.Badger
{
    public class Explode : BaseSkillState
    {
        public static float blastRadius = 16f;

        public static float blastProcCoefficient = 1f;

        public static float blastDamageCoefficient = 1.7f;

        public static float blastForce = 2000f;

        public static float exitSlowdownCoefficient = 0.2f;

        public static Vector3 blastBonusForce = new Vector3(0f, 0f, 0f);

        

        public override void OnEnter()
        {
            this.PlayAnimation("Gesture, Override", "Explode", "Shoot.playbackRate", 2.5f);

            BlastAttack blastAttack = new BlastAttack();
            {
                blastAttack.attacker = base.gameObject;
                blastAttack.baseDamage = characterBody.damage * blastDamageCoefficient;
                blastAttack.baseForce = blastForce;
                blastAttack.bonusForce = blastBonusForce;
                blastAttack.crit = RollCrit();
                blastAttack.damageType = DamageType.Generic;
                blastAttack.AddModdedDamageType(BadgerSurvivor.BadgerExplode);
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.procCoefficient = blastProcCoefficient;
                blastAttack.radius = blastRadius;
                blastAttack.position = base.gameObject.transform.position;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.teamIndex = base.teamComponent.teamIndex;

            }
            blastAttack.Fire();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}