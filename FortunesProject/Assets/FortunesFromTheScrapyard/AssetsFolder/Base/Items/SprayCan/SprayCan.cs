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
    public class SprayCan : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_SPRAYCAN_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 0)]
        public static int baseUses = 20;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, 1)]
        public static int baseCooldown = 7;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseDamageRequirement = 3f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float baseDamageCoefficient = 2f;

        public static GameObject sprayCanEffect;

        public static DamageAPI.ModdedDamageType SprayCanProc;

        public override void Initialize()
        {
            SprayCanProc = DamageAPI.ReserveDamageType();
            sprayCanEffect = AssetCollection.FindAsset<GameObject>("SprayCanEffect");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acSprayCan", ScrapyardBundle.Items);
        }

        public class SprayCanBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.SprayCan;

            private int maxUses = 0;

            private int uses;

            private bool wasConsumed = false;
            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (body && damageInfo.damage / body.damage >= 3 && !damageInfo.HasModdedDamageType(SprayCanProc))
                {
                    if(body.HasBuff(ScrapyardContent.Buffs.bdSprayCanReady))
                    {
                        body.SetBuffCount(ScrapyardContent.Buffs.bdSprayCanReady.buffIndex, 0);
                        for(int i = 0; i <= baseCooldown; i++)
                        {
                            body.AddTimedBuff(ScrapyardContent.Buffs.bdSprayCanCooldown, i);
                        }
                        if(body.GetItemCount(GetItemDef()) > 0)
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
                                procCoefficient = 1f
                            };
                            EffectManager.SimpleImpactEffect(sprayCanEffect, damageInfo.position, Vector3.up, transmit: true);
                            victimHealthComponent.TakeDamage(SprayCanDamage);
                            ConsumeUse();
                        }
                    }
                }
            }

            private void ConsumeUse()
            {
                uses--;
                wasConsumed = true;
                if(uses + baseUses == maxUses)
                {
                    body.inventory.RemoveItem(GetItemDef());
                    body.inventory.GiveItem(ScrapyardContent.Items.SprayCanConsumed);
                }
            }

            private void OnDisable()
            {
                if (NetworkServer.active && body)
                {
                    if (body.HasBuff(ScrapyardContent.Buffs.bdSprayCanReady))
                    {
                        body.SetBuffCount(ScrapyardContent.Buffs.bdSprayCanReady.buffIndex, 0);
                    }
                    if (body.HasBuff(ScrapyardContent.Buffs.bdSprayCanCooldown))
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdSprayCanCooldown);
                    }
                }

                uses = 0;
                maxUses = 0;
            }

            private void FixedUpdate()
            {
                if (maxUses > body.GetItemCount(GetItemDef()) * baseUses)
                {
                    maxUses = body.GetItemCount(GetItemDef()) * baseUses;
                    if (!wasConsumed) uses -= baseUses;
                    else wasConsumed = false;
                }
                if (maxUses < body.GetItemCount(GetItemDef()) * baseUses)
                {
                    maxUses = body.GetItemCount(GetItemDef()) * baseUses;
                    uses += baseUses;
                }

                if(NetworkServer.active && body)
                {
                    bool onCooldown = body.HasBuff(ScrapyardContent.Buffs.bdSprayCanCooldown);
                    bool ready = body.HasBuff(ScrapyardContent.Buffs.bdSprayCanReady);
                    if (!onCooldown && !ready || maxUses != body.GetBuffCount(ScrapyardContent.Buffs.bdSprayCanReady) && !onCooldown)
                    {
                        body.SetBuffCount(ScrapyardContent.Buffs.bdSprayCanReady.buffIndex, uses);
                    }
                    if (ready && onCooldown)
                    {
                        body.RemoveBuff(ScrapyardContent.Buffs.bdSprayCanReady);
                    }
                }
            }
        }
    }
}
    