using Mono.Cecil.Cil;
using MonoMod.Cil;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    public class Multitool : ScrapyardItem
    {
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool shrineBlood = true;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool shrineMountain = true;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool shrineChance = true;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool shrineCombat = false;
        [ConfigureField(ScrapyardConfig.ID_ITEMS, ConfigDescOverride = "Includes printers and shops.")]
        public static bool terminals = true;
        [ConfigureField(ScrapyardConfig.ID_ITEMS, ConfigDescOverride = "Includes chests and scavenger bags.")]
        public static bool chests = true;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static bool roulette = true;
        public override void Initialize()
        {
            /*
            On.RoR2.PurchaseInteraction.OnInteractionBegin += MultitoolInteract;
            if (terminals)
                On.RoR2.ShopTerminalBehavior.DropPickup += MultitoolTerminal;
            if (chests)
                On.RoR2.ChestBehavior.ItemDrop += MultitoolChest;
            if (roulette)
                On.RoR2.RouletteChestController.EjectPickupServer += MultitoolRoulette;
            if (shrineMountain)
                On.RoR2.ShrineBossBehavior.AddShrineStack += MultitoolMountain;
            if (shrineBlood)
                On.RoR2.ShrineBloodBehavior.AddShrineStack += MultitoolBlood;
            if (shrineChance)
                IL.RoR2.ShrineChanceBehavior.AddShrineStack += MultitoolChance;
            if (shrineCombat)
                IL.RoR2.ShrineCombatBehavior.AddShrineStack += MultitoolCombat;

            On.RoR2.CharacterMaster.OnServerStageBegin += TryRegenerateMultitool;
            */
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acMultitool", ScrapyardBundle.Indev);
        }

        private void MultitoolCombat(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<ShrineCombatBehavior>("get_monsterCredit")
                );
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, ShrineCombatBehavior, float>>((monsterCredit, self) =>
            {
                bool multitoolSuccess = false;
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        multitoolSuccess = true;

                        return monsterCredit * 2;
                    }
                }
                return monsterCredit;
            });
        }

        private void MultitoolTerminal(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        PickupDropletController.CreatePickupDroplet(self.pickupIndex,
                            (self.dropTransform ? self.dropTransform : self.transform).position,
                            self.transform.TransformVector(self.dropVelocity));
                    }
                }
            }
        }

        private void MultitoolRoulette(On.RoR2.RouletteChestController.orig_EjectPickupServer orig, RouletteChestController self, PickupIndex pickupIndex)
        {
            orig(self, pickupIndex);
            if (pickupIndex == PickupIndex.none)
            {
                return;
            }
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        PickupDropletController.CreatePickupDroplet(pickupIndex, self.ejectionTransform.position, self.ejectionTransform.rotation * (self.localEjectionVelocity + new Vector3(-2, 0, 0)));
                    }
                }
            }
        }

        private void MultitoolChance(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<PickupDropletController>(nameof(PickupDropletController.CreatePickupDroplet))
                );
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<PickupIndex, Vector3, Vector3, ShrineChanceBehavior>>((pickupIndex, dropletOrigin, dropletDirection, self) =>
            {
                bool multitoolSuccess = false;
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        multitoolSuccess = true;

                        float angle = 45;
                        Vector3 vector = dropletDirection;
                        Quaternion rotation = Quaternion.AngleAxis(-angle / 2, Vector3.up);
                        vector = rotation * vector;
                        rotation = Quaternion.AngleAxis(angle, Vector3.up);
                        for (int i = 0; i < 2; i++)
                        {
                            PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin, vector);
                            vector = rotation * vector;
                        }
                    }
                }
                if (!multitoolSuccess)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, dropletOrigin, dropletDirection);
                }
            });
        }

        private void MultitoolBlood(On.RoR2.ShrineBloodBehavior.orig_AddShrineStack orig, ShrineBloodBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.purchaseCount--;
                        self.AddShrineStack(interactor);
                    }
                }
            }

            orig(self, interactor);
        }

        private void MultitoolMountain(On.RoR2.ShrineBossBehavior.orig_AddShrineStack orig, ShrineBossBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.purchaseCount--;
                        self.AddShrineStack(interactor);
                    }
                }
            }

            orig(self, interactor);
        }

        private void MultitoolChest(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (NetworkServer.active)
            {
                MultitoolComponent mtc = null;
                if (self.TryGetComponent(out mtc))
                {
                    if (mtc.VerifyMultitoolAndBreak())
                    {
                        self.dropCount += 1;
                    }
                }
            }
            orig(self);
        }

        private void MultitoolInteract(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, RoR2.PurchaseInteraction self, RoR2.Interactor activator)
        {
            if (!self.CanBeAffordedByInteractor(activator))
            {
                return;
            }

            CharacterBody activatorBody = null;
            if (activator.gameObject.TryGetComponent(out activatorBody))
            {
                Inventory inv = activatorBody.inventory;
                int multitoolCount = inv.GetItemCount(ItemDef);
                if (multitoolCount > 0)
                {
                    MultitoolComponent mtc = self.gameObject.GetComponent<MultitoolComponent>();
                    if (mtc == null)
                        mtc = self.gameObject.AddComponent<MultitoolComponent>();

                    mtc.SetInteractor(activatorBody, inv);
                }
            }

            orig(self, activator);
        }

        private void TryRegenerateMultitool(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, RoR2.CharacterMaster self, RoR2.Stage stage)
        {
            orig(self, stage);
            if (NetworkServer.active)
            {
                int count = self.inventory.GetItemCount(ScrapyardContent.Items.MultitoolConsumed);
                if (count > 0)
                {
                    TransformMultitools(count, self);
                }
            }
        }
        private void TransformMultitools(int count, CharacterMaster master)
        {
            Inventory inv = master.inventory;
            inv.RemoveItem(ScrapyardContent.Items.MultitoolConsumed, count);
            inv.GiveItem(ItemDef, count);

            CharacterMasterNotificationQueue.SendTransformNotification(
                master, ScrapyardContent.Items.MultitoolConsumed.itemIndex,
                ItemDef.itemIndex,
                CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
        }
    }
    public class MultitoolComponent : MonoBehaviour
    {
        ItemIndex multitoolIndex => ScrapyardContent.Items.Multitool.itemIndex;
        ItemIndex multitoolBrokenIndex => ScrapyardContent.Items.MultitoolConsumed.itemIndex;
        CharacterBody lastInteractor;
        Inventory interactorInventory;

        public void SetInteractor(CharacterBody interactor, Inventory inv = null)
        {
            lastInteractor = interactor;

            if (inv == null)
                interactorInventory = interactor.GetComponent<Inventory>();
            else
                interactorInventory = inv;
        }

        bool MultitoolViable()
        {
            return interactorInventory.GetItemCount(multitoolIndex) > 0;
        }
        void BreakMultitool()
        {
            interactorInventory.RemoveItem(multitoolIndex, 1);
            interactorInventory.GiveItem(multitoolBrokenIndex, 1);

            CharacterMasterNotificationQueue.SendTransformNotification(
                lastInteractor.master, multitoolIndex,
                multitoolBrokenIndex,
                CharacterMasterNotificationQueue.TransformationType.Default);
        }
        public bool VerifyMultitoolAndBreak()
        {
            if (MultitoolViable())
            {
                BreakMultitool();
                DestroyImmediate(this);
                return true;
            }
            return false;
        }
    }
}
