using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using MSU;

namespace FortunesFromTheScrapyard.Items
{
    public class LethalInjection : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_INJECTION_DESC";
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static int toxinDurationBase = 10;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static int toxinDurationStack = 5;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float toxinInterval = 0.5f;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acLethalInjection", ScrapyardBundle.Indev);
        }

        public class LethalInjectionBehaviour : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            public static ItemDef GetItemDef() => ScrapyardContent.Items.LethalInjection;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                CharacterBody attackerBody = body;
                if (damageInfo.damage / attackerBody.damage >= 4)
                {
                    if (NetworkServer.active)
                    {
                        int injectionStacks = (int)(GetStackValue(toxinDurationBase, toxinDurationStack, stack) * damageInfo.procCoefficient);
                        //Debug.Log(injectionStacks);
                        InjectionBehavior injection = damageReport.victimBody.gameObject.EnsureComponent<InjectionBehavior>();
                        injection.hostBody = damageReport.victimBody;
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
                if (hostBody == null)
                    hostBody = GetComponent<CharacterBody>();
            }
            public void AddStacks(int stacksToAdd)
            {
                stacksRemaining += stacksToAdd;
                stopwatch = 0;
            }
            public void FixedUpdate()
            {
                if (hostBody != null && NetworkServer.active)
                {
                    if (stacksRemaining > 0)
                    {
                        stopwatch += Time.fixedDeltaTime;
                        if (stopwatch > interval)
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
}
