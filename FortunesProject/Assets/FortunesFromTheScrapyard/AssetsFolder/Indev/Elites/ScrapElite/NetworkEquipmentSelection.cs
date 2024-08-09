using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace FortunesFromTheScrapyard.Elite
{
    public class NetworkEquipmentSelection : NetworkBehaviour
    {
        public int index;
        [HideInInspector]
        public CharacterBody body;
        [HideInInspector]
        public EquipmentIndex chosenEquipmentIndex;
        [HideInInspector]
        NetworkedBodyAttachment batteryAttachment;
        [HideInInspector]
        public float baseCooldown;

        [Server]
        public void RollEquipment(CharacterBody body)
        {
            List<EquipmentIndex> list = body.isPlayerControlled ? new List<EquipmentIndex>(ScrapElite.scrapEliteEquipmentListPlayer) : new List<EquipmentIndex>(ScrapElite.scrapEliteEquipmentListEnemy);
            index = UnityEngine.Random.Range(0, list.Count - 1);
            chosenEquipmentIndex = list[index];
            baseCooldown = EquipmentCatalog.GetEquipmentDef(chosenEquipmentIndex).cooldown / 2f;

            NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
            if (!identity) return;

            new SyncDisplay(identity.netId, index, chosenEquipmentIndex).Send(R2API.Networking.NetworkDestination.Clients);
        }

        private void OnDestroy()
        {
            if ((bool)batteryAttachment)
            {
                UnityEngine.Object.Destroy(batteryAttachment.gameObject);
                batteryAttachment = null;
            }
        }

        internal class SyncDisplay : INetMessage
        {
            private NetworkInstanceId netId;
            private int index;
            private EquipmentIndex eIndex;
            public SyncDisplay()
            {
            }

            public SyncDisplay(NetworkInstanceId netId, int index, EquipmentIndex eIndex)
            {
                this.netId = netId;
                this.index = index;
                this.eIndex = eIndex;
            }

            public void Deserialize(NetworkReader reader)
            {
                this.netId = reader.ReadNetworkId();
                this.index = reader.ReadPackedIndex32();
                this.eIndex = reader.ReadEquipmentIndex();
            }

            public void OnReceived()
            {
                GameObject bodyObject = Util.FindNetworkObject(this.netId);
                if (!bodyObject)
                {
                    ScrapyardLog.Message("No Body Object");
                    return;
                }
                
                NetworkEquipmentSelection equipmentSelection = bodyObject.GetComponent<NetworkEquipmentSelection>();
                equipmentSelection.index = this.index;   
                equipmentSelection.chosenEquipmentIndex = this.eIndex;

                ScrapyardLog.Debug("Im a client that is attempting to spawn in a pickup display!!!!!!" + PickupCatalog.FindPickupIndex(this.eIndex));
                PickupDisplay pickupDisplay = bodyObject.transform.Find("ScrapAffixEquipment").GetComponent<PickupDisplay>();

                if (pickupDisplay)
                {
                    pickupDisplay.SetPickupIndex(PickupCatalog.FindPickupIndex(this.eIndex));
                    if (pickupDisplay.modelRenderer)
                    {
                        Highlight component = bodyObject.transform.Find("ScrapAffixEquipment").GetComponent<Highlight>();

                        if (component)
                        {
                            component.targetRenderer = pickupDisplay.modelRenderer;
                        }
                    }
                }
                if (this.eIndex == RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex)
                {
                    equipmentSelection.batteryAttachment = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/QuestVolatileBatteryAttachment")).GetComponent<NetworkedBodyAttachment>();
                    equipmentSelection.batteryAttachment.AttachToGameObjectAndSpawn(equipmentSelection.body.gameObject);
                }

            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(this.netId);
                writer.Write(this.index);
                writer.Write(this.eIndex);
            }
        }
    }
}