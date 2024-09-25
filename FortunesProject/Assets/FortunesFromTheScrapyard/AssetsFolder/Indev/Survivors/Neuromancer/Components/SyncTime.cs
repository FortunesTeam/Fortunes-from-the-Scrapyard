using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    internal class SyncTime : INetMessage
    {
        private NetworkInstanceId netId;
        private ulong time;
        private bool isSecondary;
        public SyncTime()
        {
        }

        public SyncTime(NetworkInstanceId netId, ulong time, bool isSecondary)
        {
            this.netId = netId;
            this.time = time;
            this.isSecondary = isSecondary;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.time = reader.ReadUInt64();
            this.isSecondary = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject)
            {
                ScrapyardLog.Message("No Body Object");
                return;
            }

            NeuromancerController neuromancerController = bodyObject.GetComponent<NeuromancerController>();
            if (neuromancerController)
            {
                neuromancerController.SapTimeEssence(time * 0.01f, isSecondary);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.time);
            writer.Write(this.isSecondary);
        }
    }
}
