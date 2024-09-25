using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2.UI;
using FortunesFromTheScrapyard.Survivors.Cloaker.Components;
using EntityStates;
using EntityStates.Cloaker.Weapon;
using RoR2.Projectile;
using RoR2.EntityLogic;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class Cloaker : ScrapyardSurvivor
    {
        public static GameObject cloakerConsumeEffect;

        public static DamageAPI.ModdedDamageType CloakerChargedDamageType;
        public static DamageAPI.ModdedDamageType CloakerAkimboDamageType;
        public override void Initialize()
        {
            CloakerChargedDamageType = DamageAPI.ReserveDamageType();
            CloakerAkimboDamageType = DamageAPI.ReserveDamageType();

            bool tempAdd(CharacterBody body) => body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked);

            CreateEffects();

            Hooks();

            TempVisualEffectAPI.AddTemporaryVisualEffect(assetCollection.FindAsset<GameObject>("CloakerMarkEffect"), tempAdd, true);

            ModifyPrefab();
        }

        public void ModifyPrefab()
        {
            var cb = characterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override ScrapyardAssetRequest<SurvivorAssetCollection> LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<SurvivorAssetCollection>("acCloaker", ScrapyardBundle.Indev);
        }
        private void CreateEffects()
        {
            cloakerConsumeEffect = assetCollection.FindAsset<GameObject>("CloakerMarkedConsumeEffect");

            CreateAndAddEffectDef(cloakerConsumeEffect);
        }
        #region projectiles
        private void CreateProjectiles()
        {
            
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
        }
        #endregion

        #region UI
        private static void CreateUI()
        {
        }
        #endregion
        private static void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            CharacterBody victimBody = obj.victimBody;
            CharacterBody attackerBody = obj.attackerBody;

            if(victimBody && attackerBody)
            {
                if (obj.damageInfo.HasModdedDamageType(CloakerAkimboDamageType) && !victimBody.HasBuff(ScrapyardContent.Buffs.bdCloakerMarkCd))
                {
                    victimBody.AddBuff(ScrapyardContent.Buffs.bdCloakerMarked);
                }
            }
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody victimBody = self.body;
            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (victimBody.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked) && attackerBody)
            {
                if (!damageInfo.HasModdedDamageType(CloakerChargedDamageType) && NetworkServer.active)
                {
                    victimBody.RemoveBuff(ScrapyardContent.Buffs.bdCloakerMarked);
                    victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdCloakerMarkCd, 5f);
                }

                if (!damageInfo.crit) damageInfo.crit = true;
                else damageInfo.damage *= 1.5f;

                EffectManager.SimpleImpactEffect(cloakerConsumeEffect, damageInfo.position, Vector3.up, transmit: true);
            }
        }

        private static void CreateAndAddEffectDef(GameObject effect)
        {
            EffectDef effectDef = new EffectDef(effect);

            ScrapyardContent.scrapyardContentPack.effectDefs.AddSingle(effectDef);
        }

        internal static void AddNetworkSoundEventDef(NetworkSoundEventDef networkSoundEventDef)
        {
            ScrapyardContent.scrapyardContentPack.networkSoundEventDefs.AddSingle(networkSoundEventDef);
        }
        internal static NetworkSoundEventDef CreateAndAddNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            AddNetworkSoundEventDef(networkSoundEventDef);

            return networkSoundEventDef;
        }
    }
}

