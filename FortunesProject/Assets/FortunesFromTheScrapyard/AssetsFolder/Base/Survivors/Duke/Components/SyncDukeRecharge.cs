using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    internal class SyncDukeRecharge : INetMessage
    {
        private NetworkInstanceId netId;
        public SyncDukeRecharge()
        {
        }

        public SyncDukeRecharge(NetworkInstanceId netId)
        {
            this.netId = netId;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject)
            {
                ScrapyardLog.Message("No Body Object");
                return;
            }

            bodyObject.GetComponent<SkillLocator>().utility.RunRecharge(2f);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
        }
    }
}
