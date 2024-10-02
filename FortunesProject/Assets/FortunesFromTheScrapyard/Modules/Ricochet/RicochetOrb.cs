using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using System.Linq;
using MSU;
using FortunesFromTheScrapyard.Survivors.Duke;
using FortunesFromTheScrapyard.Survivors.Duke.Components;

namespace FortunesFromTheScrapyard.Ricochet
{
    public class RicochetOrb : GenericDamageOrb
    {
        public float searchRadius = 9999f;
        public SphereSearch search;
        public Vector3 originalPosition;
        public int bounceCount = 1;
        public List<GameObject> hitObjects;

        public override void Begin()
        {
            this.target = PickNextTarget(this.originalPosition);

            this.duration = this.distanceToTarget / this.speed;

            Color color = Color.Lerp(Color.yellow, Color.red, bounceCount / 4f);
            float scale = Mathf.Lerp(1, 2f, bounceCount / 4f);
            EffectData effectData = new EffectData
            {
                scale = this.scale * 2f,
                origin = this.originalPosition,
                genericFloat = this.duration,
                color = color
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(DukeSurvivor.ricochetOrbEffect, effectData, true);
        }


        public override void OnArrival()
        {
            if (this.target)
            {
                HealthComponent healthComponent = target.healthComponent;
                if (healthComponent)
                {
                    var victimTrc = this.target.gameObject.EnsureComponent<TemporaryRicochetControllerVictim>();

                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = this.damageValue,
                        attacker = this.attacker,
                        force = Vector3.zero,
                        crit = this.isCrit,
                        procChainMask = this.procChainMask,
                        procCoefficient = this.procCoefficient,
                        position = this.target.transform.position,
                        damageColorIndex = this.damageColorIndex,
                    };
                    damageInfo.AddModdedDamageType(DukeSurvivor.DukeRicochet);

                    victimTrc.bounceCountStored = bounceCount;
                    victimTrc.hitObjectsStored = hitObjects;

                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                }
            }
        }

        public HurtBox PickNextTarget(Vector3 position)
        {
            HurtBox target = null;

            this.search = new SphereSearch
            {
                mask = LayerIndex.entityPrecise.mask,
                radius = searchRadius,
                origin = position
            };

            TeamMask teamMask = TeamMask.GetUnprotectedTeams(teamIndex);
            HurtBox[] hurtBoxes = search.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach (HurtBox hurtBox in hurtBoxes)
            {
                if (hurtBox.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdDukeDamageShare) && !hitObjects.Contains(hurtBox.healthComponent.body.gameObject))
                {
                    target = hurtBox;
                    break;
                }
            }
            return target;
        }
    }
}