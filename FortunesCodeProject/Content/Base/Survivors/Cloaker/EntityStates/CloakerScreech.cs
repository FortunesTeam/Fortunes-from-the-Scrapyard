using FortunesFromTheScrapyard.Survivors.Cloaker;
using FortunesFromTheScrapyard;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;
using EntityStates;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Cloaker
{
    public class CloakerScreech : BaseSkillState
    {
        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 30f, configDescOverride = "Screech damage coefficient. 6 deals 600% damage.")]
        [FormatToken(CloakerContent.SCREECHTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float DamageCoefficient = 6f;

        private const float DefaultRadius = 14f;
        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 100f, configDescOverride = "Screech blast radius in meters.")]
        [FormatToken(CloakerContent.SCREECHTOKEN, 1)]
        public static float Radius = DefaultRadius;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0.01f, 5f, configDescOverride = "Screech animation duration in seconds before attack speed scaling.")]
        public static float BaseDuration = 0.5f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 5000f, configDescOverride = "Screech knockback force.")]
        public static float Force = 700f;

        [FFTSConfigureField(FFTSConfig.ID_SURVIVORS, 0f, 5f, configDescOverride = "Screech proc coefficient.")]
        public static float ProcCoefficient = 1f;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = BaseDuration / attackSpeedStat;
            string animation = GetComponent<CloakerController>().GetAnimationStateName("Gesture, Override", "Special");
            PlayCrossfade("Gesture, Override", animation, "Special.playbackRate", duration, duration * 0.05f);
            if (!NetworkServer.active) return;
            BlastAttack blast = new BlastAttack
            {
                attacker = gameObject,
                baseDamage = damageStat * DamageCoefficient,
                baseForce = Force,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                damageType = DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = ProcCoefficient,
                radius = Radius,
                position = transform.position,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = teamComponent.teamIndex
            };
            blast.AddModdedDamageType(CloakerContent.CloakerScreechDamageType);
            blast.Fire();
            EffectManager.SpawnEffect(CloakerAssets.ScreechEffect,
                new EffectData { origin = characterBody.corePosition, rotation = Quaternion.identity, scale = Radius / DefaultRadius }, true);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Frozen;
    }
}
