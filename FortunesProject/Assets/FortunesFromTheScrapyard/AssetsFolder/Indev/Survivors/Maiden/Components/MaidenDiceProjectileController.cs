using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace FortunesFromTheScrapyard.Survivors.Maiden.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileImpactExplosion))]
    [RequireComponent(typeof(ProjectileOwnerOrbiter))]
    public class MaidenDiceProjectileController : MonoBehaviour
    {
        private ProjectileImpactExplosion explosion;

        public void OnEnable()
        {
            explosion = GetComponent<ProjectileImpactExplosion>();
            if (NetworkServer.active)
            {
                ProjectileController component = GetComponent<ProjectileController>();
                if (component.owner)
                {
                    AcquireOwner(component);
                }
                else
                {
                    component.onInitialized += AcquireOwner;
                }
            }
        }

        private void AcquireOwner(ProjectileController controller)
        {
            controller.onInitialized -= AcquireOwner;
            CharacterBody component = controller.owner.GetComponent<CharacterBody>();
            if ((bool)component)
            {
                ProjectileOwnerOrbiter component2 = GetComponent<ProjectileOwnerOrbiter>();
                component.GetComponent<MaidenController>().InitializeOrbiter(component2, this);
            }
        }

        public void Detonate()
        {
            if (explosion)
            {
                explosion.Detonate();
            }
        }
    }

}
