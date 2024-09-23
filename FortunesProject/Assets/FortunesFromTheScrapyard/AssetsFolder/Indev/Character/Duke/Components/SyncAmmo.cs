using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    internal class SyncAmmo : INetMessage
    {
        private NetworkInstanceId netId;
        private uint gauge;

        public SyncAmmo()
        {
        }

        public SyncAmmo(NetworkInstanceId netId, uint gauge)
        {
            this.netId = netId;
            this.gauge = gauge;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.gauge = reader.ReadUInt16();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject)
            {
                ScrapyardLog.Message("No Body Object");
                return;
            }

            DukeController cadetController = bodyObject.GetComponent<DukeController>();

            if (cadetController)
            {
                cadetController.ammo = (int)this.gauge;

            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.gauge);
        }
    }
}
