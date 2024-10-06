using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using RoR2;
using FortunesFromTheScrapyard;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using RoR2.Orbs;
using FortunesFromTheScrapyard.Survivors.Badger;
using R2API;

namespace EntityStates.Cloaker
{
    public class CloakerScreech : BaseSkillState
    {
        public static float baseDuration = 0.5f; 

        public static float blastRadius = 14f;

        public static float blastProcCoefficient = 1f;

        public static float blastDamageCoefficient = 600f;

        public static float blastForce = 700f;

        public static Vector3 blastBonusForce = new Vector3(0f, 0f, 0f);

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Fire();
        }

        public void Fire()
        {
            this.PlayCrossfade("Gesture, Additive", Animator.StringToHash("Special"), this.duration * 0.05f);

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

            EffectManager.SpawnEffect(FortunesFromTheScrapyard.Items.Headphones.headphonesShockwavePrefab, new EffectData
            {
                origin = characterBody.corePosition,
                rotation = Quaternion.identity,
            }, true);

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && fixedAge >= duration)
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
            return InterruptPriority.Frozen;
        }
    }
}
