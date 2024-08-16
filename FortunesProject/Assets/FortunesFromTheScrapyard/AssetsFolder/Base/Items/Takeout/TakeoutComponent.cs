using RoR2;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using MSU;
using RoR2.UI;
using R2API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using HG;

namespace FortunesFromTheScrapyard.Items
{
    public class TakeoutComponent : MonoBehaviour
    {
        public UnityEvent triggerEvents;

        public TeamIndex _teamIndex;

        public CharacterBody ownerBody;
        
        public BuffDef buff;

        private bool on = false;

        private void FixedUpdate()
        {
            if(!ownerBody.HasBuff(buff) && !on) 
            {
                base.transform.parent.Find("Radius").gameObject.SetActive(true);
                if(buff == ScrapyardContent.Buffs.bdChickenCooldown)
                {
                    ownerBody.SetBuffCount(ScrapyardContent.Buffs.bdChicken.buffIndex, 1);
                }
                on = true;
            }
        }

        public void OnTriggerStay(Collider collider)
        {
            if (!collider)
            {
                return;
            }
            CharacterBody characterBody = collider.GetComponent<CharacterBody>();
            if(!characterBody) collider.GetComponentInChildren<CharacterBody>();

            if (characterBody)
            {
                TeamComponent enemyTeam = characterBody.teamComponent;
                if(enemyTeam.teamIndex != _teamIndex && !ownerBody.HasBuff(buff))
                {
                    if (NetworkServer.active)
                    {
                        if (buff == ScrapyardContent.Buffs.bdPotstickers)
                        {
                            ownerBody.AddTimedBuff(buff, 7.5f);

                            ownerBody.healthComponent.Heal(ownerBody.healthComponent.fullCombinedHealth * Takeout.GetStackValue(Takeout.healBase, Takeout.healStack, ownerBody.GetItemCount(ScrapyardContent.Items.Takeout)), default);

                            EffectManager.SpawnEffect(Takeout.potstickerImpactEffect, new EffectData
                            {
                                origin = ownerBody.corePosition,
                            }, transmit: true);
                        }
                        else if(buff == ScrapyardContent.Buffs.bdChickenCooldown)
                        {
                            ownerBody.SetBuffCount(ScrapyardContent.Buffs.bdChicken.buffIndex, 0);

                            for (int i = 0; i <= Takeout.chickenCooldown; i++)
                            {
                                ownerBody.AddTimedBuff(buff, i);
                            }

                            EffectManager.SpawnEffect(Takeout.chickenExplosionEffect, new EffectData
                            {
                                origin = ownerBody.corePosition,
                                scale = 13,
                            }, transmit: true); 

                            BlastAttack blastAttack = new BlastAttack
                            {
                                position = ownerBody.corePosition,
                                baseDamage = ownerBody.damage * Takeout.GetStackValue(Takeout.igniteBase, Takeout.igniteStack, ownerBody.GetItemCount(ScrapyardContent.Items.Takeout)),
                                baseForce = 0f,
                                radius = 13,
                                attacker = ownerBody.gameObject,
                                inflictor = null
                            };
                            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                            blastAttack.crit = characterBody.RollCrit();
                            blastAttack.procChainMask = default;
                            blastAttack.procCoefficient = 0.5f;
                            blastAttack.damageColorIndex = DamageColorIndex.Item;
                            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                            blastAttack.damageType = DamageType.IgniteOnHit;
                            blastAttack.Fire();
                        }
                        else if(buff == ScrapyardContent.Buffs.bdNoodles)
                        {
                            ownerBody.AddTimedBuff(buff, 7.5f);
                        }
                        
                    }
                    on = false;
                    triggerEvents.Invoke();
                    Util.PlaySound("sfx_energybar_use", ownerBody.gameObject);
                }
            }
        }
    }
}

