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
                    swingComponent.Reset();
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
            private CharacterBody body;
            private Animator roughAnimator;
            private ChildLocator roughLocator;

            public static float baseSwingDuration = 0.5f;
            public float swingDuration;
            public GameObject swingInstance;
  
            private int step;

            private float timer;
            private bool startSwing;
            private bool hasFired;

            public void Reset()
            {
                this.timer = 0;
                if(this.swingInstance) Destroy(this.swingInstance);
                this.swingInstance = null;
                this.startSwing = false;
                this.roughLocator = null;
                this.roughAnimator = null;
                this.body = null;
                this.hasFired = false;
            }
            public void RoughReceptionSwing(CharacterBody characterBody) => RoughReceptionSwing(characterBody, null);

            public void RoughReceptionSwing(CharacterBody characterBody, GameObject roughObject)
            {
                if (body == null)
                {
                    body = characterBody;
                }

                if(roughObject && !roughAnimator && !roughLocator)
                {
                    roughAnimator = roughObject.transform.Find("mdlRoughReception").gameObject.GetComponent<Animator>();

                    roughLocator = roughObject.transform.Find("mdlRoughReception").gameObject.GetComponent<ChildLocator>();
                }

                this.swingDuration = RoughReceptionComponent.baseSwingDuration / characterBody.attackSpeed;

                if (roughAnimator)
                {
                    int layerIndex = roughAnimator.GetLayerIndex("Body");
                    if (layerIndex >= 0)
                    {
                        EntityState.PlayAnimationOnAnimator(roughAnimator, "Body", "Swing" + (this.step + 1), "Swing.playbackRate", swingDuration);
                    }
                }

                //ScrapyardLog.Debug("Step" + step);
                this.step = this.step == 0 ? 1 : 0;
                //ScrapyardLog.Debug("Step" + step);
                this.startSwing = true;
            }
            public void FixedUpdate()
            {
                if (this.startSwing && this.body)
                {
                    this.timer += Time.fixedDeltaTime;

                    if(this.timer >= swingDuration / 2f && !hasFired)
                    {
                        Fire();

                        this.swingInstance = UnityEngine.Object.Instantiate(roughSwingPrefab, body.corePosition + (body.transform.forward * 2f), step == 0 ? 
                            new Quaternion(0f,0f, 0f, 0f) : new Quaternion(0f, 0f, 0f, 0f));
                    }

                    if (this.timer >= swingDuration)
                    {
                        Reset();
                    }
                }
            }

            private void Fire()
            {
                //ScrapyardLog.Debug("Running Catattack?");
                if(Util.HasEffectiveAuthority(body.networkIdentity))
                {
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
                        maxDistance = 12,
                        radius = 5,
                        isCrit = body.RollCrit(),
                        muzzleName = "",
                        tracerEffectPrefab = null
                    };

                    catAttack.Fire();

                    hasFired = true;
                    //ScrapyardLog.Debug("Fired Catattack?");

                }
            }
            public void OnDisable()
            {
                Reset();
                step = 0;
            }
        }
    }
}
