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
        public static float baseDamageCoefficient = 5f;
        [HideInInspector]
        public float damageCoefficient;
        [HideInInspector] 
        public bool isCrit;
        [HideInInspector]
        public CharacterBody ownerBody;
        public CharacterBody decoyBody;
        private void Awake()
        {
            decoyBody = base.gameObject.GetComponent<CharacterBody>();
            damageCoefficient = baseDamageCoefficient;
            isCrit = false;
        }

        public void SetValuesAndKillDecoy(float damage, bool crit)
        {
            if(damage > damageCoefficient) damageCoefficient = damage;
            isCrit = crit;

            if(NetworkServer.active)
            {
                DamageInfo killDecoy = new DamageInfo();
                killDecoy.attacker = ownerBody.gameObject;
                killDecoy.inflictor = null;
                killDecoy.damage = decoyBody.healthComponent.fullCombinedHealth * 2f;
                killDecoy.procCoefficient = 0f;
                killDecoy.crit = false;
                killDecoy.damageType = DamageType.Silent | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection;
                killDecoy.damageColorIndex = DamageColorIndex.Default;
                killDecoy.force = Vector3.zero;
                killDecoy.position = decoyBody.corePosition;
                killDecoy.canRejectForce = false;
                killDecoy.rejected = false;
                killDecoy.AddModdedDamageType(DukeDecoy.DecoyHit);

                decoyBody.healthComponent.TakeDamage(killDecoy);
            }
        }
    }
}
