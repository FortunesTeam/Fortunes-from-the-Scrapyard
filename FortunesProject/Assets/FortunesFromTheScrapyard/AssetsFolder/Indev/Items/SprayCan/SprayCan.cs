using RoR2;
using RoR2.Projectile;
using RoR2.ContentManagement;
using MSU;
using MSU.Config;
using RoR2.Items;

namespace FortunesFromTheScrapyard.Items
{
    public class SprayCan : ScrapyardItem
    {
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static int baseChance = 11;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static int stackChance = 7;
        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        public static float poisonTotalDamage = 2;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acSprayCan", ScrapyardBundle.Indev);
        }

        public class SprayCanBehaviour : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            public static ItemDef GetItemDef() => ScrapyardContent.Items.SprayCan;

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                float chance = GetStackValue(baseChance, stackChance, stack);
                float finalChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(chance);
                if (Util.CheckRoll(finalChance, damageReport.attackerMaster))
                {
                    uint? maxStacksFromAttacker = null;
                    if ((damageInfo != null) ? damageInfo.inflictor : null)
                    {
                        ProjectileDamage component = damageInfo.inflictor.GetComponent<ProjectileDamage>();
                    }
                }
            }
        }
    }
}
    