using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.Skills;
using RoR2.Projectile;
using FortunesFromTheScrapyard;
using FortunesFromTheScrapyard.Items;
using MSU;
using MSU.Config;
using FortunesFromTheScrapyard.Survivors.Duke;

namespace FortunesFromTheScrapyard.Characters.DukeDecoy.Components
{
    public class DukeDecoyExplosion : MonoBehaviour
    {
        [HideInInspector]
        public float damageCoefficient = 5f;
        [HideInInspector]
        public CharacterBody ownerBody;
        public CharacterBody decoyBody;
        private void Start()
        {
            decoyBody = base.gameObject.GetComponent<CharacterBody>();
        }
        private void FixedUpdate()
        {
            if (decoyBody.healthComponent.alive == false)
            {
                Destroy(this);
            }
        }
        private void OnDestroy()
        {
            BlastAttack blastAttack = new BlastAttack();

            blastAttack.procCoefficient = 1f;
            blastAttack.attacker = ownerBody.gameObject;
            blastAttack.inflictor = null;
            blastAttack.teamIndex = ownerBody.teamComponent.teamIndex;
            blastAttack.baseDamage = ownerBody.damage * damageCoefficient;
            blastAttack.baseForce = 500f;
            blastAttack.position = decoyBody.corePosition;
            blastAttack.radius = 12;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            // blastAttack.bonusForce = Vector3.zero;
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.damageColorIndex = DamageColorIndex.Default;
            blastAttack.Fire();

            EffectManager.SpawnEffect(Headphones.headphonesShockwavePrefab, new EffectData
            {
                origin = decoyBody.corePosition,
                rotation = Quaternion.identity,
                scale = 1f
            }, true);
        }

    }
}
