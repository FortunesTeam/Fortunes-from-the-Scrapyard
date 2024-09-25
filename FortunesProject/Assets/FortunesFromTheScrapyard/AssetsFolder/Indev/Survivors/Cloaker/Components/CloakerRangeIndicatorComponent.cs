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
        public UnityEvent triggerEvents;

        public TeamIndex _teamIndex;

        public CharacterBody ownerBody;

        public CloakerController cloakerController;

        public bool on => cloakerController.passiveCloakOn && cloakerController.graceTimer <= 0f;

        private void FixedUpdate()
        {
            if(on)
            {
                base.transform.parent.Find("Radius").gameObject.SetActive(true);
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
                if(enemyTeam.teamIndex != _teamIndex && ownerBody.hasCloakBuff && on)
                {
                    if (NetworkServer.active)
                    {
                        ownerBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                        cloakerController.passiveCloakOn = false;
                    }

                    triggerEvents.Invoke();
                }
            }
        }
    }
}

