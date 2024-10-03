using HG;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace FortunesFromTheScrapyard
{
    [RequireComponent(typeof(SphereCollider))]
    public class DisableCollisionsIfInTrigger : MonoBehaviour
    {
        public Collider colliderToIgnore;

        private SphereCollider trigger;

        private TeamIndex allyTeamIndex;

        private GameObject owner;

        private bool isIgnoring;

        public void Awake()
        {
            trigger = GetComponent<SphereCollider>();
        }

        public void Start()
        {
            TryGetComponent<ProjectileController>(out var projController);
            allyTeamIndex = projController.teamFilter.teamIndex;
            owner = projController.owner;
        }

        private void FixedUpdate()
        {
            foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
            {
                if (body.teamComponent.teamIndex != allyTeamIndex && body.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare))
                {
                    Physics.IgnoreCollision(colliderToIgnore, owner.GetComponent<Collider>(), false);
                    return;
                }
            }

            Physics.IgnoreCollision(colliderToIgnore, owner.GetComponent<Collider>(), true);
        }
    }
}


