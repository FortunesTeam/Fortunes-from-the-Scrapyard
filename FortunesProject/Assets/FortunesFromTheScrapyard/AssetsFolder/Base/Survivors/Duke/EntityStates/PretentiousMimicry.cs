using RoR2;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Duke.Components;
using FortunesFromTheScrapyard.Characters.DukeDecoy;
using FortunesFromTheScrapyard.Characters.DukeDecoy.Components;
using UnityEngine;
using System;
using FortunesFromTheScrapyard.Survivors.Duke;
using UnityEngine.Events;

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

            base.PlayCrossfade("Gesture, Override", "DeployDecoy", 0.05f);

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
                CharacterSpawnCard decoySpawnCard = FortunesFromTheScrapyard.Characters.DukeDecoy.DukeDecoy.cscDukeDecoy;
                decoySpawnCard.inventoryToCopy = base.characterBody.inventory;
                DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
                {
                    position = base.characterBody.previousPosition,
                    placementMode = DirectorPlacementRule.PlacementMode.Direct
                };
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(decoySpawnCard, directorPlacementRule, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = TeamIndex.Player;
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.summonerBodyObject = base.gameObject;
                directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult result)
                {
                    if(result.success && result.spawnedInstance)
                    {
                        CharacterMaster master = result.spawnedInstance.GetComponent<CharacterMaster>();
                        Deployable deployable = result.spawnedInstance.AddComponent<Deployable>();
                        characterBody.master.AddDeployable(deployable, DukeSurvivor.CloneSlot);
                        deployable.onUndeploy = deployable.onUndeploy ?? new UnityEvent();
                        deployable.onUndeploy.AddListener(master.TrueKill);
                        GameObject bodyObject = master.GetBodyObject();
                        bodyObject.GetComponent<DukeDecoyExplosion>().ownerBody = characterBody;
                    }
                });
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                UnityEngine.Object.Destroy(decoySpawnCard);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
