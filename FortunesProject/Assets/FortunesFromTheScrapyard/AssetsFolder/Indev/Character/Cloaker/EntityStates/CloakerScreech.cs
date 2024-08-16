using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Cloaker;
using RoR2;
using FortunesFromTheScrapyard;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using RoR2.Orbs;

namespace EntityStates.Cloaker
{
    public class CloakerScreech : BaseSkillState
    {
        private CloakerTrackerController tracker;

        private HurtBox victim;
        public override void OnEnter()
        {
            base.OnEnter();

            HurtBox[] hurtBoxes = new SphereSearch
            {
                origin = characterBody.corePosition,
                radius = 9999f,
                mask = LayerIndex.entityPrecise.mask
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            foreach(HurtBox hurtBox in hurtBoxes)
            {
                if(hurtBox.healthComponent.body)
                {
                    if(hurtBox.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked))
                    {
                        OrbManager.instance.AddOrb(new LightningStrikeOrb
                        {
                            attacker = base.gameObject,
                            damageColorIndex = DamageColorIndex.Default,
                            damageValue = characterBody.damage * 4f,
                            isCrit = Util.CheckRoll(characterBody.crit, characterBody.master),
                            procChainMask = default(ProcChainMask),
                            procCoefficient = 0.7f,
                            target = hurtBox
                        });
                    }
                }
            }

            if (base.isAuthority) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
