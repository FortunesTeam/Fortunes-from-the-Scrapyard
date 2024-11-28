using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using FortunesFromTheScrapyard.Monsters.Gardener;

namespace EntityStates.Gardener
{
    public class GardenerBarrage : BaseSkillState
    {
        private float stopwatch;

        private float missileStopwatch = 0;

        public static float baseDuration = 4f;

        public static string muzzleString = "FaceMissileCenter";

        public float missileSpawnFrequency = 2f;

        public static float missileSpawnDelay = 0f;

        public static float missileForce = 1000f;

        public static float damageCoefficient = 1f;

        public static float maxSpread = 90f;

        public static GameObject projectilePrefab = GardenerMonster.wispProjectilePrefab;

        public static GameObject muzzleflashPrefab;

        public static string jarEffectChildLocatorString;

        public static string jarOpenSoundString;

        public static string jarCloseSoundString;

        public static GameObject jarOpenEffectPrefab;

        public static GameObject jarCloseEffectPrefab;

        private ChildLocator childLocator;

        private static int BeginGravekeeperBarrageStateHash = Animator.StringToHash("BeginGravekeeperBarrage");

        private static int EndGravekeeperBarrageStateHash = Animator.StringToHash("EndGravekeeperBarrage");

        private bool isAnimate;

        private bool isFirst = true;

        private bool isLast = false;

        public override void OnEnter()
        {
            base.OnEnter();
            missileSpawnFrequency *= attackSpeedStat;
            missileStopwatch -= missileSpawnDelay;
            /*Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                if ((bool)childLocator)
                {
                    childLocator.FindChild("JarEffectLoop").gameObject.SetActive(value: true);
                }
            }
            PlayAnimation("Jar, Override", BeginGravekeeperBarrageStateHash);
            EffectManager.SimpleMuzzleFlash(jarOpenEffectPrefab, base.gameObject, jarEffectChildLocatorString, transmit: false);
            Util.PlaySound(jarOpenSoundString, base.gameObject);*/
            base.characterBody.SetAimTimer(baseDuration + 2f);
            isAnimate = true;
        }

        private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
        {
            projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, maxSpread, 1f, 1f);
            Vector2 randomCircleSpread = UnityEngine.Random.insideUnitCircle;
            projectileRay.origin += new Vector3(randomCircleSpread.x, 0f, randomCircleSpread.y);
            //Vector2 randomCircleSpread2 = UnityEngine.Random.insideUnitCircle * 50f;
            //projectileRay.direction += new Vector3(randomCircleSpread2.x, 0f, randomCircleSpread2.y);
            //EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, muzzleString, transmit: false);
            if (NetworkServer.active)
            {
                ProjectileManager.instance.FireProjectile(projectilePrefab, projectileRay.origin, Util.QuaternionSafeLookRotation(projectileRay.direction), base.gameObject, damageStat * damageCoefficient, missileForce, Util.CheckRoll(critStat, base.characterBody.master));
            }
        }

        public override void OnExit()
        {
            /*PlayCrossfade("Jar, Override", EndGravekeeperBarrageStateHash, 0.06f);
            EffectManager.SimpleMuzzleFlash(jarCloseEffectPrefab, base.gameObject, jarEffectChildLocatorString, transmit: false);
            Util.PlaySound(jarCloseSoundString, base.gameObject);
            if ((bool)childLocator)
            {
                childLocator.FindChild("JarEffectLoop").gameObject.SetActive(value: false);
            }*/
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float deltaTime = GetDeltaTime();
            stopwatch += deltaTime;
            missileStopwatch += deltaTime;
            if (stopwatch >= (baseDuration - (2f / missileSpawnFrequency)))
                isLast = true;
            if (missileStopwatch >= 1f / missileSpawnFrequency)
            {
                missileStopwatch -= 1f / missileSpawnFrequency;
                //Transform transform = childLocator.FindChild(muzzleString);
                if (isAnimate)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        PlayCrossfade("FullBody, Override", "PrimaryLoop", "Primary.playbackRate", 2f / missileSpawnFrequency, 0.5f / missileSpawnFrequency);
                    }
                    else if (isLast)
                        PlayCrossfade("FullBody, Override", "PrimaryEnd", "Primary.playbackRate", 2f / missileSpawnFrequency, 0.1f / missileSpawnFrequency);
                    else
                    PlayCrossfade("FullBody, Override", "PrimaryLoop", "Primary.playbackRate", 2f / missileSpawnFrequency, 0.1f / missileSpawnFrequency);
                    isAnimate = false;
                }
                else
                    isAnimate = true;
                
                if ((bool)transform)
                {
                    Ray projectileRay = default(Ray);
                    projectileRay.origin = transform.position + new Vector3(0, 3, 0) + characterDirection.forward.normalized * -2;
                    projectileRay.direction = GetAimRay().direction;
                    float maxDistance = 1000f;
                    if (Physics.Raycast(GetAimRay(), out var hitInfo, maxDistance, LayerIndex.world.mask))
                    {
                        projectileRay.direction = hitInfo.point - transform.position;
                    }
                    FireBlob(projectileRay, 60f, 60f);
                }
            }
            if (stopwatch >= baseDuration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
