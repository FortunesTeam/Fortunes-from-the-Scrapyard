using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates;

namespace FortunesFromTheScrapyard
{
    public class RoughReception : ScrapyardItem
    {
        public const string TOKEN = "SCRAPYARD_ITEM_ROUGHRECEPTION_DESC";

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float swingBaseDamageCoefficient = 2.5f;

        [ConfigureField(ScrapyardConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float swingDamageCoefficientPerStack = 2.5f;

        public static GameObject roughSwingPrefab;
        public override void Initialize()
        {
            roughSwingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSlash.prefab").WaitForCompletion().InstantiateClone("RoughSwingPrefab");

            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig.Invoke(self, skill);

            if(self.HasItem(ScrapyardContent.Items.RoughReception) && skill == self.skillLocator.primary)
            {
                RoughReceptionSwing swingComponent = self.gameObject.GetComponent<RoughReceptionSwing>();
                List<GameObject> itemDisplayObjects = self.modelLocator.modelTransform.GetComponent<CharacterModel>().GetItemDisplayObjects(ScrapyardContent.Items.RoughReception.itemIndex);

                if(itemDisplayObjects.Count > 0)
                {
                    GameObject roughObject = itemDisplayObjects[0];

                    Animator roughAnimator = roughObject.gameObject.GetComponent<Animator>();



                    ChildLocator roughLocator = roughObject.transform.Find("mdlRoughReception").gameObject.GetComponent<ChildLocator>();

                    Ray aimRay;
                    if (self.inputBank) aimRay = new Ray(self.inputBank.aimOrigin, self.inputBank.aimDirection);
                    else aimRay = new Ray(self.transform.position, self.transform.forward);

                    BulletAttack catAttack = new BulletAttack
                    {
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        owner = self.gameObject,
                        weapon = null,
                        bulletCount = 1,
                        damage = self.damage * GetStackValue(swingBaseDamageCoefficient, swingDamageCoefficientPerStack, self.GetItemCount(ScrapyardContent.Items.RoughReception)),
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        force = 400f,
                        HitEffectNormal = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 0.7f,
                        maxDistance = 7,
                        radius = 10,
                        isCrit = self.RollCrit(),
                        muzzleName = "",
                        tracerEffectPrefab = null
                    };

                    catAttack.Fire();

                    int layerIndex = roughAnimator.GetLayerIndex("Body");
                    if (layerIndex >= 0)
                    {
                        EntityState.PlayAnimationOnAnimator(roughAnimator, "Body", "Swing" + swingComponent.step, "Swing.playbackRate", 1f);
                    }

                    swingComponent.swingInstance = UnityEngine.Object.Instantiate(roughSwingPrefab, roughLocator.FindChild("Swing" + swingComponent.step));
                    swingComponent.swingDuration = RoughReceptionSwing.baseSwingDuration / self.attackSpeed;

                    swingComponent.step = swingComponent.step == 1 ? 2 : 1;
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override ScrapyardAssetRequest LoadAssetRequest()
        {
            return ScrapyardAssets.LoadAssetAsync<ItemAssetCollection>("acRoughReception", ScrapyardBundle.Items);
        }

        public class RoughReceptionBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => ScrapyardContent.Items.RoughReception;
            private void OnEnable()
            {
                body.gameObject.EnsureComponent<RoughReceptionSwing>();
            }
            private void OnDisable()
            {
                body.gameObject.GetComponent<RoughReceptionSwing>().enabled = false;
            }
        }
        public class RoughReceptionSwing : NetworkBehaviour
        {
            public static float baseSwingDuration = 2f;
            public float swingDuration;
            public GameObject swingInstance;
            [SyncVar]   
            public int step = 1;

            private float timer;

            public void FixedUpdate()
            {
                if(swingInstance)
                {
                    timer += Time.fixedDeltaTime;

                    if(timer >= swingDuration)
                    {
                        UnityEngine.Object.Destroy(swingInstance);
                        timer = 0f;
                    }
                }
            }
        }
    }
}
