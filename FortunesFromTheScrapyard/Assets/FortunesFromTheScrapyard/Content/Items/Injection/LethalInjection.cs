using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FortunesFromTheScrapyard.Modules.HitHooks;
using FortunesFromTheScrapyard.Modules;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Items
{
    public class LethalInjection : ItemBase
    {
        [AutoConfig("Toxin Duration Base", 10)]
        public static int toxinDurationBase = 10;
        [AutoConfig("Toxin Duration Stack", 5)]
        public static int toxinDurationStack = 5;
        [AutoConfig("Toxin Interval", 0.5f)]
        public static float toxinInterval = 0.5f;
        public override void Init()
        {
            itemName = "LethalInjection";
            base.Init();
        }
        public override void Hooks()
        {
            GetHitBehavior += InjectionOnHit;
        }

        private void InjectionOnHit(CharacterBody attackerBody, DamageInfo damageInfo, CharacterBody victimBody)
        {
            if(damageInfo.damage / attackerBody.damage >= 4)
            {
                int injectionCount = GetCount(attackerBody);
                if (injectionCount > 0 && NetworkServer.active)
                {
                    int injectionStacks = (int)(GetStackValue(toxinDurationBase, toxinDurationStack, injectionCount) * damageInfo.procCoefficient);
                    //Debug.Log(injectionStacks);
                    InjectionBehavior injection = victimBody.GetComponent<InjectionBehavior>();
                    if (injection == null)
                        injection = victimBody.gameObject.AddComponent<InjectionBehavior>();
                    injection.hostBody = victimBody;
                    injection.AddStacks(injectionStacks);
                }
            }
        }
    }

    public class InjectionBehavior : MonoBehaviour
    {
        internal CharacterBody hostBody;
        float interval => LethalInjection.toxinInterval;
        float stopwatch = 0;
        int stacksRemaining = 0;
        public void Start()
        {
            if(hostBody == null)
                hostBody = GetComponent<CharacterBody>();
        }
        public void AddStacks(int stacksToAdd)
        {
            stacksRemaining += stacksToAdd;
            stopwatch = 0;
        }
        public void FixedUpdate()
        {
            if(hostBody != null && NetworkServer.active)
            {
                if (stacksRemaining > 0)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if(stopwatch > interval)
                    {
                        //create effect
                        hostBody.AddBuff(RoR2Content.Buffs.PermanentCurse);
                        stacksRemaining--;
                        stopwatch -= interval;
                    }
                }
                else
                {
                    stopwatch = 0;
                }
            }
        }
    }
}
