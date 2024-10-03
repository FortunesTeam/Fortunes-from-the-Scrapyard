using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.Orbs;
using FortunesFromTheScrapyard.Survivors.Duke;
using System.Collections.Generic;

namespace FortunesFromTheScrapyard.Ricochet
{
    public class TemporaryRicochetControllerVictim : MonoBehaviour
    {
        public NetworkSoundEventDef ricochetSound;

        public bool canRicochet = false;
        public float ricochetMultiplier = DukeSurvivor.damageShareCoefficient;
        public int bounceCountStored = 1;
        private DamageInfo damageInfo;

        public List<GameObject> hitObjectsStored;
        public void FixedUpdate()
        {
            if (damageInfo != null && damageInfo.attacker && canRicochet)
            {
                this.canRicochet = false;
                TeamComponent teamComponent = damageInfo.attacker.GetComponent<TeamComponent>();

                RicochetOrb orb = new RicochetOrb
                {
                    originalPosition = base.transform.position,
                    origin = base.transform.position,
                    speed = 180f + (20f * bounceCountStored),
                    attacker = this.damageInfo.attacker,
                    damageValue = this.damageInfo.damage * this.ricochetMultiplier,
                    teamIndex = teamComponent.teamIndex,
                    procCoefficient = this.damageInfo.procCoefficient,
                    isCrit = this.damageInfo.crit,
                    bounceCount = bounceCountStored,
                    hitObjects = hitObjectsStored,
                    damageColorIndex = DamageColorIndex.WeakPoint
                };

                OrbManager.instance.AddOrb(orb);

                EffectData effectData = new EffectData
                {
                    origin = base.transform.position,
                    scale = 1f
                };
                EffectManager.SpawnEffect(DukeSurvivor.ricochetImpact, effectData, transmit: true);
                //EffectManager.SimpleSoundEffect(this.ricochetSound.index, base.transform.position, true);

                Destroy(this);
            }

        }
        public void RicochetBullet(DamageInfo damageInfo)
        {
            hitObjectsStored.Add(base.gameObject);

            if (this.damageInfo != null)
            {
                damageInfo.GetModdedDamageTypeHolder().CopyTo(this.damageInfo);
                this.damageInfo.damage = damageInfo.damage * DukeSurvivor.damageShareCoefficient;
                canRicochet = true;
                return;
            }
            this.damageInfo = damageInfo;
            this.damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;
            this.bounceCountStored++;
            canRicochet = true;
        }
    }
}
