using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoR2;
using RoR2.Orbs;
using UnityEngine.Networking;
using R2API;
using RoR2.Projectile;
using R2API.Networking;
using R2API.Networking.Interfaces;

namespace FortunesFromTheScrapyard.Orbs
{
    public class NeedleOrb : Orb
    {
        public float speed = 100f;
        public float damageValue;
        public GameObject attacker;
        public GameObject inflictor;
        public int bouncesRemaining;
        public List<HealthComponent> bouncedObjects;
        public TeamIndex teamIndex;
        public bool isCrit;
        public ProcChainMask procChainMask;
        public float procCoefficient = 1f;
        public DamageColorIndex damageColorIndex;
        public float range = 20f;
        public float damageCoefficientPerBounce = 1f;
        public int targetsToFindPerBounce = 1;
        public DamageType damageType;

        private bool canBounceOnSameTarget;
        private bool failedToKill;
        private BullseyeSearch search;
        private GameObject weaponInstance;
        private EntityStateMachine outer = null;

        public override void Begin()
        {
            base.duration = base.distanceToTarget / this.speed;
            this.canBounceOnSameTarget = false;
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/HuntressGlaiveOrbEffect"), effectData, true);
        }

        public override void OnArrival()
        {
            if (this.target)
            {
                HealthComponent healthComponent = this.target.healthComponent;
                if (healthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = this.damageValue;
                    damageInfo.attacker = this.attacker;
                    damageInfo.inflictor = this.inflictor;
                    damageInfo.force = Vector3.zero;
                    damageInfo.crit = this.isCrit;
                    damageInfo.procChainMask = this.procChainMask;
                    damageInfo.procCoefficient = this.procCoefficient;
                    damageInfo.position = this.target.transform.position;
                    damageInfo.damageColorIndex = this.damageColorIndex;
                    damageInfo.damageType = this.damageType;
                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                }
                this.failedToKill |= (!healthComponent || healthComponent.alive);

                if (this.bouncesRemaining > 0)
                {
                    for (int i = 0; i < this.targetsToFindPerBounce; i++)
                    {
                        if (this.bouncedObjects != null)
                        {
                            if (this.canBounceOnSameTarget)
                            {
                                this.bouncedObjects.Clear();
                            }
                            this.bouncedObjects.Add(this.target.healthComponent);
                        }
                        HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
                        if (hurtBox)
                        {
                            NeedleOrb needleOrb = new NeedleOrb();
                            needleOrb.search = this.search;
                            needleOrb.origin = this.target.transform.position;
                            needleOrb.target = hurtBox;
                            needleOrb.attacker = this.attacker;
                            needleOrb.inflictor = this.inflictor;
                            needleOrb.teamIndex = this.teamIndex;
                            needleOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
                            needleOrb.bouncesRemaining = this.bouncesRemaining - 1;
                            needleOrb.isCrit = this.isCrit;
                            needleOrb.bouncedObjects = this.bouncedObjects;
                            needleOrb.procChainMask = this.procChainMask;
                            needleOrb.procCoefficient = this.procCoefficient;
                            needleOrb.damageColorIndex = this.damageColorIndex;
                            needleOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
                            needleOrb.speed = this.speed;
                            needleOrb.range = this.range;
                            needleOrb.damageType = this.damageType;
                            needleOrb.failedToKill = this.failedToKill;
                            OrbManager.instance.AddOrb(needleOrb);
                        }
                    }
                }
                else
                {
                    DropNeedle(this.target.transform.position);
                }
            }
        }
        private void DropNeedle(Vector3 location)
        {
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
               // projectilePrefab = //DealerAssets.needlePickupProjectile,
                position = location + Vector3.up * 2f,
                rotation = Quaternion.identity,
                owner = attacker,
                damage = 0,
                force = 0,
                crit = false,
                speedOverride = 0f
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);

            bouncesRemaining++;
        }
        public HurtBox PickNextTarget(Vector3 position)
        {
            if (this.search == null)
            {
                this.search = new BullseyeSearch();
            }
            this.search.searchOrigin = position;
            this.search.searchDirection = Vector3.zero;
            this.search.teamMaskFilter = TeamMask.allButNeutral;
            this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
            this.search.filterByLoS = false;
            this.search.sortMode = BullseyeSearch.SortMode.Distance;
            this.search.maxDistanceFilter = this.range;
            this.search.RefreshCandidates();
            HurtBox hurtBox = (from v in this.search.GetResults()
                               where !this.bouncedObjects.Contains(v.healthComponent)
                               select v).FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                this.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }
    }
}