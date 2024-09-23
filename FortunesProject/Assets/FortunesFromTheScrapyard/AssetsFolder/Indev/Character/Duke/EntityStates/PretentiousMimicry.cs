using RoR2;
using System;
using EntityStates;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using FortunesFromTheScrapyard.Survivors.Duke;

namespace EntityStates.Duke
{
    public class PretentiousMimicry : BaseSkillState
    {
        private DukeController dukeController;
        private float duration = 0.5f;
        public override void OnEnter()
        {
            dukeController = base.gameObject.GetComponent<DukeController>();
            base.OnEnter();

            base.PlayCrossfade("Gesture, Override", "Special", 0.05f);

            Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            if(NetworkServer.active)
            {
                MasterSummon masterSummon = new MasterSummon();
                masterSummon.masterPrefab = DukeSurvivor.dukeDecoyMasterPrefab;
                //masterSummon.masterPrefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<SkillLocator>().primary.skillDef = base.skillLocator.primary.skillDef;
                masterSummon.ignoreTeamMemberLimit = true;  
                masterSummon.teamIndexOverride = base.teamComponent.teamIndex;
                masterSummon.summonerBodyObject = base.gameObject;
                masterSummon.position = base.characterBody.previousPosition;
                masterSummon.rotation = Util.QuaternionSafeLookRotation(base.characterBody.characterDirection.forward);
                masterSummon.inventoryToCopy = base.characterBody.inventory;

                CharacterMaster decoyMaster;
                decoyMaster = masterSummon.Perform();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
