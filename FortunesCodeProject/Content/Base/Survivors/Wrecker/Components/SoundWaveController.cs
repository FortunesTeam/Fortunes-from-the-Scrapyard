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
    public class SoundWaveController : MonoBehaviour
    {
        private float timer = 0f;

        private int maxProjectiles = 5;

        private int currentProjectiles;

        private float interval = 0.05f;

        ProjectileController projectileController;

        private SphereCollider sphereCollider;

        private BuffWard buffWard;

        public GameObject soundScape => WreckerSurvivor.soundScape;

        private void Start()
        {
            projectileController = base.gameObject.GetComponent<ProjectileController>();

            buffWard = soundScape.GetComponent<BuffWard>();
            buffWard.radius = 2.5f;

            sphereCollider = soundScape.gameObject.GetComponent<SphereCollider>();
            sphereCollider.radius = 2.5f;

            currentProjectiles = maxProjectiles;
        }
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= interval && currentProjectiles > 0)
            {
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);

                fireProjectileInfo.projectilePrefab = soundScape;
                fireProjectileInfo.position = base.gameObject.transform.position;
                fireProjectileInfo.rotation = base.gameObject.transform.rotation;
                fireProjectileInfo.owner = projectileController.owner;

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                currentProjectiles--;
                buffWard.radius += 2.5f;
                sphereCollider.radius += 2.5f;
                interval += 0.025f;
                timer -= interval;
            }
            else if (currentProjectiles <= 0)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }
    }
}
