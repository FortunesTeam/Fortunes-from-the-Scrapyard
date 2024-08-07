using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

using System.Collections.Generic;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class PunchEffectComponent : MonoBehaviour
    {
        public List<DamageInfo> damageInfos = new List<DamageInfo>();

        private Transform targetTransform;
        private CharacterBody characterBody;

        private int punchCount = 0;
        private float timeBetweenPunches = 0.08f;
        private bool hasStartedLooping = false;
        private float punchStopwatch = 0f;

        private void Start()
        {
            this.targetTransform = base.gameObject.transform;
            this.characterBody = base.gameObject.GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            if(this.hasStartedLooping) this.punchStopwatch -= Time.fixedDeltaTime;

            if (this.hasStartedLooping && !this.targetTransform || this.punchCount <= 0)
            {
                Destroy(this);
                return;
            }

            if (this.hasStartedLooping && this.punchStopwatch <= 0f)
            {
                this.Punch();
            }
        }

        public void StartLooping()
        {
            punchCount = damageInfos.Count;
            hasStartedLooping = true;
        }
        private void Punch()
        {
            if(NetworkServer.active)
            {
                characterBody.healthComponent.TakeDamage(damageInfos[punchCount - 1]);
                GlobalEventManager.instance.OnHitEnemy(damageInfos[punchCount - 1], base.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfos[punchCount - 1], base.gameObject);

                EffectManager.SpawnEffect(Neuromancer.punchImpactEffect, new EffectData
                {
                    origin = this.gameObject.transform.position,
                    scale = 1.5f
                }, false);

                Util.PlaySound("sfx_neuromancer_punch", characterBody.gameObject);

                this.punchCount--;
                this.punchStopwatch = this.timeBetweenPunches;
            }
        }
    }
}
