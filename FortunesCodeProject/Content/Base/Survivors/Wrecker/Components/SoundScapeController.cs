using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates.Wrecker;
using System;
using RoR2.Projectile;
using FortunesFromTheScrapyard.Survivors.Wrecker;
using FortunesFromTheScrapyard;

namespace EntityStates.Wrecker.Components
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(BuffWard))]
    [RequireComponent(typeof(ProjectileSimple))]
    public class SoundScapeController : MonoBehaviour
    {
        private SphereCollider collider;
        private BuffWard buffWard;
        private ProjectileSimple projectileSimple;
        private Transform areaIndicator;

        private float baseColliderRadius;
        private float baseBuffWardRadius;
        private Vector3 baseAreaIndicator;

        private float startValue;
        private float endValue;

        public void Awake()
        {
            collider = GetComponent<SphereCollider>();
            buffWard = GetComponent<BuffWard>();
            projectileSimple = GetComponent<ProjectileSimple>();
            //areaIndicator = transform.Find("AreaIndicator");
        }

        public void Start()
        {
            baseColliderRadius = collider.radius;
            baseBuffWardRadius = buffWard.radius;
            baseAreaIndicator = areaIndicator.localScale;

            collider.radius = 0f;
            buffWard.radius = 0f;
            //areaIndicator.localScale = Vector3.zero;


            Keyframe[] keyframes = projectileSimple.velocityOverLifetime.GetKeys();
            startValue = keyframes[0].value;
            endValue = keyframes[1].value;
        }

        public void FixedUpdate()
        {
            //float timeScaledSize = Mathf.InverseLerp(0, 2, Mathf.Clamp(projectileSimple.stopwatch, 0f, 2f));
            float timeScaledSize = buffWard.radiusCoefficientCurve.Evaluate(projectileSimple.stopwatch);
            collider.radius = baseColliderRadius * timeScaledSize;
            buffWard.radius = baseBuffWardRadius * timeScaledSize;
            projectileSimple.SetForwardSpeed(6f * (1f - timeScaledSize));
            //areaIndicator.localScale = baseAreaIndicator * timeScaledSize;
        }
    }
}
