using RoR2;
using RoR2.Projectile;
using RoR2.ContentManagement;
using MSU;
using MSU.Config;
using RoR2.Items;
using static FortunesFromTheScrapyard.Items.LethalInjection;
using UnityEngine.Networking;
using UnityEngine;
using R2API;
using UnityEngine.UIElements;

namespace FortunesFromTheScrapyard.Items
{
    public class SprayCan : FFTSItem
    {
        public const string TOKEN = "FFTS_ITEM_SPRAYCAN_DESCRIPTION";
        public const string TOKEN2 = "FFTS_ITEM_SPRAYCAN_PICKUP";

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        [FormatToken(TOKEN2, 0)]
        public static int baseUses = 10;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static int baseCooldown = 5;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseDamageRequirement = 4.0f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseDamageCoefficient = 2.75f;

        public static GameObject sprayCanEffect;

        public override void Initialize()
        {
            FFTSContent.ProcType.SprayCan = ProcTypeAPI.ReserveProcType();
            sprayCanEffect = assetCollection.FindAsset<GameObject>("SprayCanEffect");

            On.RoR2.CharacterMaster.OnServerStageBegin += TryRegenerateSprayCan;
        }

        private void TryRegenerateSprayCan(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            if (NetworkServer.active)
            {
                int count = self.inventory.GetItemCount(FFTSContent.Items.SprayCanConsumed);
                if (count > 0)
                {
                    TransformSprayCans(count, self);
                }
            }
        }

        private void TransformSprayCans(int count, CharacterMaster master)
        {
            Inventory inv = master.inventory;
            inv.RemoveItem(FFTSContent.Items.SprayCanConsumed, count);
            inv.GiveItem(FFTSContent.Items.SprayCan, count);

            CharacterMasterNotificationQueue.SendTransformNotification(
                master, FFTSContent.Items.SprayCanConsumed.itemIndex,
                FFTSContent.Items.SprayCan.itemIndex,
                CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acSprayCan", FFTSBundle.Items);
        }

        public class SprayCanBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => FFTSContent.Items.SprayCan;

            private int maxUses = 0;

            private int uses;

            private bool wasConsumed = false;
            private void OnEnable()
            {
                if (!NetworkServer.active)
                    return;

                maxUses = body.GetItemCount(GetItemDef()) * baseUses;
                if (uses == 0) uses = maxUses;
                body.SetBuffCount(FFTSContent.Buffs.bdSprayCanReady.buffIndex, uses);
            }
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (body && damageInfo.damage / body.damage >= baseDamageRequirement && !damageInfo.procChainMask.HasModdedProc(FFTSContent.ProcType.SprayCan))
                {
                    if (body.HasBuff(FFTSContent.Buffs.bdSprayCanReady))
                    {
                        body.SetBuffCount(FFTSContent.Buffs.bdSprayCanReady.buffIndex, 0);
                        for (int i = 0; i <= baseCooldown; i++)
                        {
                            body.AddTimedBuff(FFTSContent.Buffs.bdSprayCanCooldown, i);
                        }
                        if (body.GetItemCount(GetItemDef()) > 0)
                        {
                            DamageInfo SprayCanDamage = new DamageInfo
                            {
                                damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, baseDamageCoefficient),
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageType.Generic,
                                attacker = damageInfo.attacker,
                                crit = damageInfo.crit,
                                force = Vector3.zero,
                                inflictor = null,
                                position = damageInfo.position,
                                procChainMask = damageInfo.procChainMask,
                                procCoefficient = 0f
                            };
                            EffectManager.SimpleImpactEffect(sprayCanEffect, damageInfo.position, Vector3.up, transmit: true);
                            SprayCanDamage.procChainMask.AddModdedProc(FFTSContent.ProcType.SprayCan);
                            victimHealthComponent.TakeDamage(SprayCanDamage);
                            GlobalEventManager.instance.OnHitEnemy(SprayCanDamage, victimHealthComponent.gameObject);
                            GlobalEventManager.instance.OnHitAllProcess(SprayCanDamage, victimHealthComponent.gameObject);
                            ConsumeUse();
                        }
                    }
                }
            }

            private void ConsumeUse()
            {
                uses--;
                wasConsumed = true;
                if (uses <= maxUses - baseUses)
                {
                    body.inventory.RemoveItem(GetItemDef());
                    body.inventory.GiveItem(FFTSContent.Items.SprayCanConsumed);

                    if (body.master)
                    {
                        CharacterMasterNotificationQueue.SendTransformNotification(body.master, FFTSContent.Items.SprayCan.itemIndex, FFTSContent.Items.SprayCanConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    }
                }
            }

            private void OnDisable()
            {
                if (NetworkServer.active && body)
                {
                    if (body.HasBuff(FFTSContent.Buffs.bdSprayCanReady))
                    {
                        body.SetBuffCount(FFTSContent.Buffs.bdSprayCanReady.buffIndex, 0);
                    }
                    if (body.HasBuff(FFTSContent.Buffs.bdSprayCanCooldown))
                    {
                        body.RemoveBuff(FFTSContent.Buffs.bdSprayCanCooldown);
                    }
                }
            }

            private void FixedUpdate()
            {
                
                if (maxUses > stack * baseUses)
                {
                    maxUses = stack * baseUses;
                    if (!wasConsumed) uses -= baseUses;
                    else wasConsumed = false;
                }
                if (maxUses < stack * baseUses)
                {
                    maxUses = stack * baseUses;
                    uses += baseUses;
                }

                if (NetworkServer.active && body)
                {
                    bool onCooldown = body.HasBuff(FFTSContent.Buffs.bdSprayCanCooldown);
                    bool ready = body.HasBuff(FFTSContent.Buffs.bdSprayCanReady);
                    if (!onCooldown && !ready || maxUses != body.GetBuffCount(FFTSContent.Buffs.bdSprayCanReady) && !onCooldown)
                    {
                        body.SetBuffCount(FFTSContent.Buffs.bdSprayCanReady.buffIndex, uses + (10 * (stack - 1)));
                    }
                    if (ready && onCooldown)
                    {
                        body.RemoveBuff(FFTSContent.Buffs.bdSprayCanReady);
                    }
                }
            }
        }
    }
}
