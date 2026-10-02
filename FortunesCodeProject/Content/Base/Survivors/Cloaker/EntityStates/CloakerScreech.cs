using FortunesFromTheScrapyard.Survivors.Cloaker;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Cloaker
{
    public class CloakerScreech : BaseSkillState
    {
        public const float DamageCoefficient = 6f;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = 0.5f / attackSpeedStat;
            string animation = GetComponent<CloakerController>().GetAnimationStateName("Gesture, Override", "Special");
            PlayCrossfade("Gesture, Override", animation, "Special.playbackRate", duration, duration * 0.05f);
            if (!NetworkServer.active) return;
            BlastAttack blast = new BlastAttack
            {
                attacker = gameObject,
                baseDamage = damageStat * DamageCoefficient,
                baseForce = 700f,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                damageType = DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = 1f,
                radius = 14f,
                position = transform.position,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = teamComponent.teamIndex
            };
            blast.AddModdedDamageType(CloakerContent.CloakerScreechDamageType);
            blast.Fire();
            EffectManager.SpawnEffect(CloakerAssets.ScreechEffect,
                new EffectData { origin = characterBody.corePosition, rotation = Quaternion.identity }, true);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Frozen;
    }
}
