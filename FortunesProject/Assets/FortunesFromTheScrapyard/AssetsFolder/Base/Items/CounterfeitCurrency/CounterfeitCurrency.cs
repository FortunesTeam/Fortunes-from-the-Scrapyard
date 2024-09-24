
using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using MSU.Config;
using MSU;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    public class CounterfeitCurrency : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_COUNTERFEITCURRENCY_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float commonChestLifePercent = 0.25f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float uncommonChestLifePercent = 0.5f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float rareChestLifePercent = 0.8f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 3)]
        public static int minCommonCost = 1;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 4)]
        public static int minUncommonCost = 50;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 5)]
        public static int minRareCost = 250;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 6)]
        public static int maxChests = 3;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 7)]
        public static int maxChestsStack = 3;

        public override void Initialize()
        {
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
            On.RoR2.ShrineColossusAccessBehavior.OnInteraction += ShrineColossusAccessBehavior_OnInteraction;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if(!self.CanBeAffordedByInteractor(activator))
            {
                return;
            }

            CharacterBody activatorBody = activator.GetComponent<CharacterBody>();

            if(self.cost > activatorBody.master.money)
            {
                Inventory inv = activatorBody.inventory;
                int counterfeitCount = inv.GetItemCount(ScrapyardContent.Items.CounterfeitCurrency);
                if (counterfeitCount > 0 && activatorBody.HasBuff(ScrapyardContent.Buffs.bdCounterfeitLimit))
                {
                    if ((self.costType & CostTypeIndex.Money) != 0)
                    {
                        int common = Run.instance.GetDifficultyScaledCost(minCommonCost, Stage.instance.entryDifficultyCoefficient);
                        int uncommon = Run.instance.GetDifficultyScaledCost(minUncommonCost, Stage.instance.entryDifficultyCoefficient);
                        int rare = Run.instance.GetDifficultyScaledCost(minRareCost, Stage.instance.entryDifficultyCoefficient);
                        HealthComponent activatorHealthComponent = activator.GetComponent<HealthComponent>();
                        if (activatorHealthComponent)
                        {
                            if (self.cost >= common && self.cost < uncommon)
                            {
                                CounterfeitCalculations(self, activatorHealthComponent, commonChestLifePercent);
                            }
                            else if (self.cost >= uncommon && self.cost < rare)
                            {
                                CounterfeitCalculations(self, activatorHealthComponent, uncommonChestLifePercent);
                            }
                            else
                            {
                                CounterfeitCalculations(self, activatorHealthComponent, rareChestLifePercent);
                            }
                        }
                    }
                }
            }

            int actualCount = 0;
            if (activatorBody != null)
            {
                actualCount = activatorBody.GetBuffCount(DLC2Content.Buffs.SoulCost);
            }

            orig.Invoke(self, activator);

            if (self.costType == CostTypeIndex.SoulCost && activatorBody)
            {
                activatorBody.SetBuffCount(DLC2Content.Buffs.SoulCost.buffIndex, actualCount + self.cost / 10);
            }
        }

        private void ShrineColossusAccessBehavior_OnInteraction(On.RoR2.ShrineColossusAccessBehavior.orig_OnInteraction orig, ShrineColossusAccessBehavior self, Interactor interactor)
        {
            Dictionary<CharacterBody, int> trueSoulCostValues = new Dictionary<CharacterBody, int>();
            CharacterBody interactorBody = interactor.GetComponent<CharacterBody>();

            if (interactorBody && interactorBody.master.playerCharacterMasterController)
            {
                foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances)
                {
                    CharacterBody characterBody = instance.master.GetBody();
                    if (characterBody && characterBody != interactorBody)
                    {
                        trueSoulCostValues.Add(characterBody, characterBody.GetBuffCount(DLC2Content.Buffs.SoulCost) + self.purchaseInteraction.cost / 10);
                    }
                }
            }

            orig.Invoke(self, interactor);

            foreach (KeyValuePair<CharacterBody, int> entry in trueSoulCostValues)
            {
                entry.Key.SetBuffCount(DLC2Content.Buffs.SoulCost.buffIndex, entry.Value);
            }
        }

        private bool PurchaseInteraction_CanBeAffordedByInteractor(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            var canPurchase = orig.Invoke(self, activator);

            CharacterBody activatorBody = null;

            if (!canPurchase && activator.gameObject.TryGetComponent(out activatorBody))
            {
                Inventory inv = activatorBody.inventory;
                int counterfeitCount = inv.GetItemCount(ScrapyardContent.Items.CounterfeitCurrency);
                if (counterfeitCount > 0 && activatorBody.HasBuff(ScrapyardContent.Buffs.bdCounterfeitLimit))
                {
                    if ((self.costType & CostTypeIndex.Money) != 0)
                    {
                        int common = Run.instance.GetDifficultyScaledCost(minCommonCost, Stage.instance.entryDifficultyCoefficient);
                        int uncommon = Run.instance.GetDifficultyScaledCost(minUncommonCost, Stage.instance.entryDifficultyCoefficient);
                        int rare = Run.instance.GetDifficultyScaledCost(minRareCost, Stage.instance.entryDifficultyCoefficient);
                        HealthComponent activatorHealthComponent = activator.GetComponent<HealthComponent>();
                        if (activatorHealthComponent)
                        {
                            if (self.cost >= common && self.cost < uncommon)
                            {
                                canPurchase = true;
                            }
                            else if (self.cost >= uncommon && self.cost < rare)
                            {
                                canPurchase = true;
                            }
                            else
                            {
                                canPurchase = true;
                            }
                        }
                    }
                }
            }

            return canPurchase;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acCounterfeitCurrency", ScrapyardBundle.Items);
        }

        public void CounterfeitCalculations(PurchaseInteraction self, HealthComponent activatorHealthComponent, float value)
        {
            float combinedHealth = activatorHealthComponent.combinedHealth;
            float num2 = activatorHealthComponent.fullCombinedHealth * value;
            int newCount = activatorHealthComponent.body.GetBuffCount(DLC2Content.Buffs.SoulCost) + (int)(value * 10);
            if (combinedHealth > num2)
            {
                activatorHealthComponent.body.SetBuffCount(DLC2Content.Buffs.SoulCost.buffIndex, newCount);
                activatorHealthComponent.body.AddBuff(DLC2Content.Buffs.FreeUnlocks);

                Util.PlaySound("sfx_lunarmoney_start", activatorHealthComponent.gameObject);
            }

            if(NetworkServer.active)
            {
                activatorHealthComponent.body.RemoveBuff(ScrapyardContent.Buffs.bdCounterfeitLimit);
            }
        }

        public bool CounterfeitCanPurchase(PurchaseInteraction self, HealthComponent activatorHealthComponent, float value)
        {
            float combinedHealth = activatorHealthComponent.combinedHealth;
            float num2 = activatorHealthComponent.fullCombinedHealth * value;
            int newCount = activatorHealthComponent.body.GetBuffCount(DLC2Content.Buffs.SoulCost) + (int)(value * 10);
            if (combinedHealth > num2)
            {
                return true;
            }
            return false;

        }

        public class CounterfeitCurrencyBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.CounterfeitCurrency;

            private void OnEnable()
            {
                if(NetworkServer.active)
                {
                    body.SetBuffCount(ScrapyardContent.Buffs.bdCounterfeitLimit.buffIndex, (int)GetStackValue(maxChests, maxChestsStack, body.GetItemCount(GetItemDef())));
                }
            }

            private void OnDisable()
            {
                if(NetworkServer.active)
                {
                    body.SetBuffCount(ScrapyardContent.Buffs.bdCounterfeitLimit.buffIndex, 0);
                }
            }
        }
    }
}
