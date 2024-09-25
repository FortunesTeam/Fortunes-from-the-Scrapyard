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
                RoughReceptionComponent swingComponent = self.gameObject.GetComponent<RoughReceptionComponent>();
                List<GameObject> itemDisplayObjects = self.modelLocator.modelTransform.GetComponent<CharacterModel>().GetItemDisplayObjects(ScrapyardContent.Items.RoughReception.itemIndex);
                
                if(itemDisplayObjects.Count > 0)
                {
                    swingComponent.RoughReceptionSwing(self, itemDisplayObjects[0]);
                }
                else swingComponent.RoughReceptionSwing(self);
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
                body.gameObject.EnsureComponent<RoughReceptionComponent>();
            }
            private void OnDisable()
            {
                body.gameObject.GetComponent<RoughReceptionComponent>().enabled = false;
            }
        }
        public class RoughReceptionComponent : MonoBehaviour
        {
            private Animator roughAnimator;

            private ChildLocator roughLocator;

            public static float baseSwingDuration = 2f;
            public float swingDuration;
            public GameObject swingInstance;
            
            public int step = 0;

            private float timer;

            public void Start()
            {
            }
            public void RoughReceptionSwing(CharacterBody body) => RoughReceptionSwing(body, null);

            public void RoughReceptionSwing(CharacterBody body, GameObject roughObject)
            {
                if(roughObject && !roughAnimator && !roughLocator)
                {
                    roughAnimator = roughObject.transform.Find("mdlRoughReception").gameObject.GetComponent<Animator>();

                    roughLocator = roughObject.transform.Find("mdlRoughReception").gameObject.GetComponent<ChildLocator>();
                }

                Ray aimRay;
                if (body.inputBank) aimRay = new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
                else aimRay = new Ray(body.transform.position, body.transform.forward);

                BulletAttack catAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = body.gameObject,
                    weapon = null,
                    bulletCount = 1,
                    damage = body.damage * GetStackValue(swingBaseDamageCoefficient, swingDamageCoefficientPerStack, body.GetItemCount(ScrapyardContent.Items.RoughReception)),
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = 400f,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0.7f,
                    maxDistance = 7,
                    radius = 10,
                    isCrit = body.RollCrit(),
                    muzzleName = "",
                    tracerEffectPrefab = null
                };

                catAttack.Fire();

                if (roughAnimator)
                {
                    int layerIndex = roughAnimator.GetLayerIndex("Body");
                    if (layerIndex >= 0)
                    {
                        EntityState.PlayAnimationOnAnimator(roughAnimator, "Body", "Swing" + (this.step + 1), "Swing.playbackRate", 1f);
                    }
                }

                this.swingInstance = UnityEngine.Object.Instantiate(roughSwingPrefab, body.corePosition + (body.transform.forward * 2f), Util.QuaternionSafeLookRotation(body.inputBank.aimDirection));
                this.swingDuration = RoughReceptionComponent.baseSwingDuration / body.attackSpeed;

                this.step = this.step == 0 ? 0 : 1;
            }
            public void FixedUpdate()
            {
                if (swingInstance)
                {
                    timer += Time.fixedDeltaTime;

                    if (timer >= swingDuration)
                    {
                        UnityEngine.Object.Destroy(swingInstance);
                        timer = 0f;
                    }
                }
            }
            public void OnDisable()
            {
                roughLocator = null;
                roughAnimator = null;
            }
        }
    }
}
