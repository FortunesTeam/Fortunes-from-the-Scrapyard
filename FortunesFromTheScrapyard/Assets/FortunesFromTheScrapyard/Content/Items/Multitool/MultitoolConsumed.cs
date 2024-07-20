using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    public class MultitoolConsumed : ItemBase<MultitoolConsumed>
    {
        public override void Init()
        {
            itemName = "MultitoolConsumed";
            base.Init();
        }
        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += TryRegenerateMultitool;
        }

        private void TryRegenerateMultitool(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, RoR2.CharacterMaster self, RoR2.Stage stage)
        {
            orig(self, stage);
            if (NetworkServer.active)
            {
                int count = GetCount(self);
                if (count > 0)
                {
                    TransformMultitools(count, self);
                }
            }
        }
        private void TransformMultitools(int count, CharacterMaster master)
        {
            Inventory inv = master.inventory;
            inv.RemoveItem(instance.GetItemDef(), count);
            inv.GiveItem(Multitool.instance.GetItemDef(), count);

            CharacterMasterNotificationQueue.SendTransformNotification(
                master, instance.GetItemDef().itemIndex,
                Multitool.instance.GetItemDef().itemIndex,
                CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
        }
    }
}
