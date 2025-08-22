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

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerRangeIndicatorComponent : MonoBehaviour
    {
        [HideInInspector]
        public TeamIndex _teamIndex;
        [HideInInspector]
        public CharacterBody ownerBody;
        [HideInInspector]
        public CloakerController cloakerController;

        public GameObject passiveCloakOnPrefab;

        public GameObject passiveCloakOffPrefab;

        private bool _cloakOn;
        private bool cloakOn
        {
            get 
            { 
                return _cloakOn;
            } 
            set
            {
                if (_cloakOn != value)
                {
                    _cloakOn = value;
                    if (passiveCloakOnPrefab)
                    {
                        passiveCloakOnPrefab.SetActive(value);
                    }
                    if (passiveCloakOffPrefab)
                    {
                        passiveCloakOffPrefab.SetActive(!value);
                    }
                }
            }
        }
        public bool on => cloakerController.passiveCloakOn && cloakerController.graceTimer <= 0f;
        private void FixedUpdate()
        {
            if (!cloakOn && cloakerController.passiveCloakOn)
            {
                cloakOn = true;
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (!other || !cloakOn)
            {
                return;
            }
            CharacterBody characterBody = other.GetComponent<CharacterBody>();
            if (!characterBody) 
            { 
                characterBody = other.GetComponentInChildren<CharacterBody>(); 
            }

            if (characterBody)
            {
                TeamComponent enemyTeam = characterBody.teamComponent;
                TeamMask enemyMask = TeamMask.GetEnemyTeams(_teamIndex);
                enemyMask.RemoveTeam(TeamIndex.Neutral);
                if (enemyTeam && enemyMask.HasTeam(enemyTeam.teamIndex))
                {
                    cloakerController.DeactivateCloak();
                    cloakOn = false;
                }
            }
        }
    }
}

