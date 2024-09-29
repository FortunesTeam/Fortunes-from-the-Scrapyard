using UnityEngine;
using EntityStates;
// using BadgerMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
// using BadgerMod.Badger.Content;
using UnityEngine.Networking;
using EntityStates.Badger.Components;
using static RoR2.OverlapAttack;
using System;
using RoR2.Projectile;
using FortunesFromTheScrapyard.Survivors.Badger;

namespace EntityStates.Badger
{
    public class SoundScape : BaseSkillState
    {
        private float baseDuration = 0.5f;
        public GameObject soundWave => BadgerSurvivor.soundWave;
        private static float baseSoundWaveDamageCoefficient = 6f;
        public override void OnEnter()
        {

            this.PlayCrossfade("Gesture, Override", "BeatDrop", "Special.playbackRate", baseDuration * 2f, baseDuration * 0.05f);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = soundWave,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = base.gameObject,
                    damage = this.damageStat * baseSoundWaveDamageCoefficient,
                    force = 0f,
                    crit = base.RollCrit(),

                };

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority && fixedAge >= baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}