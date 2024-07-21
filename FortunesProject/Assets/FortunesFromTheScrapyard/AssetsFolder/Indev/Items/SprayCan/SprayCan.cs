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
        private static DotBuffDef _dotBuffDef;
        public override void Initialize()
        {
            _dotBuffDef = AssetCollection.FindAsset<DotBuffDef>("dbdSprayCan");
            _dotBuffDef.Init();
            _dotBuffDef.DotDef.damageColorIndex = DamageColorIndex.Poison;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acSprayCan", ScrapyardBundle.Indev);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_dotBuffDef);
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
                        if (component && component.useDotMaxStacksFromAttacker)
                        {
                            maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                        }
                    }

                    InflictDotInfo inflictDotInfo = new InflictDotInfo
                    {
                        attackerObject = damageInfo.attacker,
                        victimObject = damageReport.victimBody.gameObject,
                        totalDamage = new float?(damageInfo.damage * poisonTotalDamage),
                        damageMultiplier = 1f,
                        dotIndex = _dotBuffDef.DotIndex,
                        maxStacksFromAttacker = maxStacksFromAttacker
                    };
                    DotController.InflictDot(ref inflictDotInfo);
                }
            }
        }
    }
}
