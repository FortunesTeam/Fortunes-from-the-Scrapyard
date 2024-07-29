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

namespace FortunesFromTheScrapyard.Items
{
    public class RangeIndicator : MonoBehaviour
    {
        public UnityEvent triggerEvents;

        public TeamIndex _teamIndex;

        public CharacterBody ownerBody;
        
        public BuffDef buff;

        private bool on = true;

        private void FixedUpdate()
        {
            if(!ownerBody.HasBuff(buff) && !on) 
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
            CharacterBody characterBody = collider.transform.root.gameObject.GetComponent<CharacterBody>();
            if(!characterBody) collider.transform.root.gameObject.GetComponentInChildren<CharacterBody>();
            if (characterBody)
            {
                TeamComponent enemyTeam = characterBody.teamComponent;
                if(enemyTeam.teamIndex != _teamIndex && !ownerBody.HasBuff(buff))
                {
                    if (NetworkServer.active)
                    {
                        ownerBody.AddTimedBuff(buff, 4f);

                        if (buff == ScrapyardContent.Buffs.bdPotstickers)
                        {
                            characterBody.AddTimedBuff(RoR2Content.Buffs.Slow60, 4);
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

