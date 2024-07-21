using RoR2;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2.Items;
using MSU;

namespace FortunesFromTheScrapyard.Items
{
    public class Headphones : ScrapyardItem
    {
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float chanceBase = 17f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float chanceStack = 10f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float disorientDuration = 2f;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float disorientDamage = 0.2f;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acHeadphones", ScrapyardBundle.Indev);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public class HeadphonesBehaviour : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.Headphones;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                var victimBody = damageReport.victimBody;
                if (!victimBody.HasBuff(ScrapyardContent.Buffs.bdDisorient))
                {
                    float procChance = GetStackValue(chanceBase, chanceStack, stack) * damageReport.damageInfo.procCoefficient;
                    float adjustedProcChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
                    if (Util.CheckRoll(adjustedProcChance, damageReport.attackerMaster))
                    {
                        victimBody.AddTimedBuff(ScrapyardContent.Buffs.bdDisorient, disorientDuration);
                    }
                }
            }
        }

        public class HeadphonesBuffBehaviour : BaseBuffBehaviour, IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => ScrapyardContent.Buffs.bdDisorient;
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if(CharacterBody.HasBuff(ScrapyardContent.Buffs.bdDisorient))
                {
                    damageInfo.damage *= 1 + disorientDamage;
                }
            }
        }
    }
}
