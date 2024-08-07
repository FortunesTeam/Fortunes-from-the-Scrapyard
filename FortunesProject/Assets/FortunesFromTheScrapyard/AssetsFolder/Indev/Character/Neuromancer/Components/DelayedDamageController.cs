using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using FortunesFromTheScrapyard.Survivors;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class DelayedDamageController : MonoBehaviour
    {
        private CharacterBody victimBody;

        public List<DamageInfo> damageInfos = new List<DamageInfo>();

        public List<Vector3> attackedVector = new List<Vector3>();

        private bool hasFired;

        private void Awake()
        {
            this.victimBody = base.gameObject.GetComponent<CharacterBody>();
        }

        private void Start()
        {

        }
        private void FixedUpdate()
        {
            if(NetworkServer.active && !victimBody.HasBuff(ScrapyardContent.Buffs.bdTimeStopped) && !hasFired)
            {
                Fire();
                hasFired = true;
            }
        }

        private void Fire()
        {
            List<DamageInfo> heldDamageInfos = new List<DamageInfo>(damageInfos);
            foreach(var dinfo in heldDamageInfos.Select((value, i) => new {i, value})) 
            {
                var value = dinfo.value;
                var index = dinfo.i;

                if(value.HasModdedDamageType(Neuromancer.DelayedSecondary))
                {
                    value.RemoveModdedDamageType(Neuromancer.DelayedSecondary);
                    value.rejected = false;

                    victimBody.healthComponent.TakeDamage(value);
                    GlobalEventManager.instance.OnHitEnemy(value, victimBody.gameObject);
                    GlobalEventManager.instance.OnHitAll(value, victimBody.gameObject);
                    NeuromancerController nController = value.attacker.GetComponent<NeuromancerController>();
                    if (nController)
                    {
                        TimeSiphonOrb timeOrb = new TimeSiphonOrb();
                        timeOrb.origin = base.transform.position;
                        timeOrb.target = value.attacker.GetComponent<CharacterBody>().mainHurtBox;
                        timeOrb.siphonValue = nController.maxTimeEssence / 4f;
                        timeOrb.overrideDuration = 0.3f;
                        timeOrb.isSecondarySiphon = true;
                        OrbManager.instance.AddOrb(timeOrb);
                    }
                }
                else if(value.HasModdedDamageType(Neuromancer.DelayedUtility))
                {
                    value.RemoveModdedDamageType(Neuromancer.DelayedUtility);
                    value.rejected = false;

                    victimBody.healthComponent.TakeDamage(value);
                    GlobalEventManager.instance.OnHitEnemy(value, victimBody.gameObject);
                    GlobalEventManager.instance.OnHitAll(value, victimBody.gameObject);

                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.position = value.position;
                    fireProjectileInfo.rotation = Quaternion.LookRotation(attackedVector[index]);
                    fireProjectileInfo.crit = value.crit;
                    fireProjectileInfo.damage = 0.5f * value.attacker.GetComponent<CharacterBody>().damage;
                    fireProjectileInfo.owner = value.attacker;
                    fireProjectileInfo.projectilePrefab = Neuromancer.timeZapCone;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                else if(value.HasModdedDamageType(Neuromancer.DelayedPrimary)) 
                {
                    value.RemoveModdedDamageType(Neuromancer.DelayedPrimary);
                    value.rejected = false;

                    victimBody.healthComponent.TakeDamage(value);
                    GlobalEventManager.instance.OnHitEnemy(value, victimBody.gameObject);
                    GlobalEventManager.instance.OnHitAll(value, victimBody.gameObject);
                }
                else if(value.HasModdedDamageType(Neuromancer.DelayedPunch))
                {
                    value.RemoveModdedDamageType(Neuromancer.DelayedPunch);
                    value.rejected = false;
                    value.damageType |= DamageType.Stun1s;

                    if (!victimBody.gameObject.GetComponent<PunchEffectComponent>()) victimBody.gameObject.AddComponent<PunchEffectComponent>();

                    PunchEffectComponent punchEffectComponent = victimBody.gameObject.GetComponent<PunchEffectComponent>();

                    punchEffectComponent.damageInfos.Add(value);
                }
                else
                {
                    value.rejected = false;
                    victimBody.healthComponent.TakeDamage(value);
                    GlobalEventManager.instance.OnHitEnemy(value, victimBody.gameObject);
                    GlobalEventManager.instance.OnHitAll(value, victimBody.gameObject);
                }
            }

            if(victimBody.gameObject.GetComponent<PunchEffectComponent>())
            {
                victimBody.gameObject.GetComponent<PunchEffectComponent>().StartLooping();
            }

            UnityEngine.Component.Destroy(this);
        }
        private void OnDestroy()
        {

        }
    }
}
