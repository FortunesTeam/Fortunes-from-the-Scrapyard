
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class TimeSiphonOrb : Orb
    {
        public float siphonValue;

        public bool scaleOrb = true;

        public bool isSecondarySiphon;

        public float overrideDuration = 0.3f;

        public override void Begin()
        {
            if (target)
            {
                base.duration = overrideDuration;
                float scale = (scaleOrb ? Mathf.Min(siphonValue, target.healthComponent.body.gameObject.GetComponent<NeuromancerController>().maxTimeEssence) : target.healthComponent.body.gameObject.GetComponent<NeuromancerController>().maxTimeEssence);
                EffectData effectData = new EffectData
                {
                    scale = scale,
                    origin = origin,
                    genericFloat = base.duration
                };
                effectData.SetHurtBoxReference(target);
                EffectManager.SpawnEffect(NeuromancerSurvivor.timeSiphonOrbEffect, effectData, transmit: true);
            }
        }

        public override void OnArrival()
        {
            if (target)
            {
                NetworkIdentity identity = target.healthComponent.body.gameObject.GetComponent<NetworkIdentity>();
                if (!identity) return;

                ulong time = (ulong)(siphonValue * 100f);

                new SyncTime(identity.netId, time, isSecondarySiphon).Send(R2API.Networking.NetworkDestination.Clients);

            }
        }
    }
}
