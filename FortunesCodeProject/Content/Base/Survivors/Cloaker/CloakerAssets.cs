using System;
using System.Collections.Generic;
using MSU;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    internal static class CloakerAssets
    {
        internal const string ChargeSound = "Play_sfx_cloaker_charge";
        internal const string ChargedShotSound = "Play_sfx_cloaker_chargeshot";
        internal const string ShotSound = "Play_sfx_cloaker_shot";
        internal const string PingSound = "Play_sfx_cloaker_ping";
        internal const string ScreamSound = "Play_sfx_cloaker_scream";
        internal static NetworkSoundEventDef StealthSound;
        internal static GameObject RailTracer, RailImpact, ChargeCrosshair, GoldTracer, CritTracer;
        internal static GameObject PistolMuzzle, PistolImpact;
        internal static GameObject RangeIndicator, TrackingIndicator, ConsumeEffect, ScreechEffect;

        internal static void Initialize(SurvivorAssetCollection collection)
        {
            StealthSound = FFTSContent.CreateAndAddNetworkSoundEventDef("Play_sfx_cloaker_stealth");
            GoldTracer = LoadAddress<GameObject>("RoR2/Base/GoldGat/TracerGoldGat.prefab");
            CritTracer = LoadAddress<GameObject>("RoR2/Base/Captain/TracerCaptainShotgun.prefab");
            PistolMuzzle = LoadAddress<GameObject>("RoR2/Base/Common/VFX/Muzzleflash1.prefab");
            PistolImpact = LoadAddress<GameObject>("RoR2/Base/Commando/HitsparkCommando.prefab");
            RailTracer = LoadAddress<GameObject>("RoR2/DLC1/Railgunner/TracerRailgun.prefab");
            RailImpact = LoadAddress<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgun.prefab");
            ChargeCrosshair = LoadAddress<GameObject>("RoR2/Base/Mage/MageCrosshair.prefab");
            RangeIndicator = collection.FindAsset<GameObject>("CloakerRangeIndicator");
            TrackingIndicator = collection.FindAsset<GameObject>("CloakerTrackingIndicator");
            ConsumeEffect = collection.FindAsset<GameObject>("CloakerMarkedConsumeEffect");
            ScreechEffect = collection.FindAsset<GameObject>("CloakerScreechEffect");
            ScreechEffect.GetComponent<EffectComponent>().soundName = ScreamSound;
            GameObject mark = collection.FindAsset<GameObject>("CloakerMarkEffect");
            if (!TempVisualEffectAPI.AddTemporaryVisualEffect(mark,
                body => body.HasBuff(FFTSContent.Buffs.bdCloakerMarked), true))
                throw new InvalidOperationException("Failed to register Cloaker's marked visual effect.");

            GameObject vanilla = LoadAddress<GameObject>("RoR2/Base/NearbyDamageBonus/NearbyDamageBonusIndicator.prefab");
            AssignMaterial("Donut", "Donut");
            AssignMaterial("Radius", "Radius, Spherical");
            void AssignMaterial(string target, string source)
            {
                Material material = new Material(vanilla.transform.Find(source).GetComponent<MeshRenderer>().sharedMaterial);
                material.SetColor("_TintColor", new Color(0.2358491f, 0.1768868f, 0.2268582f));
                RangeIndicator.transform.Find(target).GetComponent<MeshRenderer>().sharedMaterial = material;
            }
        }

        internal static T LoadAddress<T>(string key) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T asset = handle.WaitForCompletion();
            if (handle.Status != AsyncOperationStatus.Succeeded || !asset)
                throw new InvalidOperationException($"Required Cloaker Addressable '{key}' failed to load.", handle.OperationException);
            return asset;
        }

        internal static void ConfigureAudio(GameObject body)
        {
            if (Application.isBatchMode) return;
            AkGameObj emitter = body.GetComponent<AkGameObj>() ?? body.AddComponent<AkGameObj>();
            emitter.isStaticObject = false;
            emitter.enabled = true;
            CopyBanks("RoR2/Base/Bandit2/Bandit2Body.prefab");
            CopyBanks("RoR2/Base/Huntress/HuntressBody.prefab");
            void CopyBanks(string key)
            {
                AkBank[] sources = LoadAddress<GameObject>(key).GetComponents<AkBank>();
                if (sources.Length == 0)
                    throw new InvalidOperationException($"Cloaker audio donor '{key}' has no banks.");
                foreach (AkBank source in sources)
                {
                    if (source.data == null || !source.data.IsValid() || source.triggerList == null || source.unloadTriggerList == null)
                        throw new InvalidOperationException($"Cloaker audio donor '{key}' has an incomplete bank.");
                    AkBank bank = null;
                    foreach (AkBank existing in body.GetComponents<AkBank>())
                        if (existing.data != null && existing.data.IsValid() && existing.data.Name == source.data.Name)
                        {
                            bank = existing;
                            break;
                        }
                    if (!bank) bank = body.AddComponent<AkBank>();
                    bank.data = new AK.Wwise.Bank { WwiseObjectReference = source.data.WwiseObjectReference };
                    bank.triggerList = new List<int>(source.triggerList);
                    bank.unloadTriggerList = new List<int>(source.unloadTriggerList);
                    bank.useOtherObject = false;
                    bank.decodeBank = source.decodeBank;
                    bank.saveDecodedBank = source.saveDecodedBank;
                    bank.overrideLoadSetting = true;
                    bank.loadAsynchronous = false;
                    bank.enabled = true;
                }
            }
        }
    }
}
