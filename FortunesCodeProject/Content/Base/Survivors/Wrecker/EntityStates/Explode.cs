using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
// using WreckerMod.Wrecker.Content;
using EntityStates.Wrecker.Components;
using R2API;
// using WreckerMod.Modules.BaseStates;
using KinematicCharacterController;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Wrecker;

namespace EntityStates.Wrecker
{
    public class Explode : BaseSkillState
    {
        public static float baseDuration = 0.75f; //per wave

        public static float blastRadius = 13f;

        public static float blastProcCoefficient = 1f;

        public static float blastDamageCoefficient = 3.0f;

        public static float blastForce = 700f;

        public static Vector3 blastBonusForce = new Vector3(0f, 0f, 0f);

        public static int maxBursts = 3;

        private float duration;
        private int burstsFired;
        private float timer;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;

            Fire();
        }

        public void Fire()
        {
            burstsFired++;

            this.PlayCrossfade("Gesture, Override", "Explode", "Secondary.playbackRate", this.duration, this.duration * 0.05f);

            BlastAttack blastAttack = new BlastAttack();
            {
                blastAttack.attacker = base.gameObject;
                blastAttack.baseDamage = characterBody.damage * blastDamageCoefficient;
                blastAttack.baseForce = blastForce * burstsFired;
                blastAttack.bonusForce = blastBonusForce;
                blastAttack.crit = RollCrit();
                blastAttack.damageType = DamageType.Generic;
                blastAttack.AddModdedDamageType(WreckerSurvivor.WreckerExplode);
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.procCoefficient = blastProcCoefficient;
                blastAttack.radius = blastRadius * burstsFired;
                blastAttack.position = base.gameObject.transform.position;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.teamIndex = base.teamComponent.teamIndex;

            }
            blastAttack.Fire();

            EffectManager.SpawnEffect(FortunesFromTheScrapyard.Items.BrokenHeadphones.headphonesShockwavePrefab, new EffectData
            {
                origin = characterBody.corePosition,
                rotation = Quaternion.identity,
                scale = 0.6f * burstsFired
            }, true);

        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.fixedDeltaTime;

            if (timer >= this.duration && burstsFired < maxBursts)
            {
                timer = 0f;

                this.Fire();
            }

            if (base.isAuthority && burstsFired == maxBursts)
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