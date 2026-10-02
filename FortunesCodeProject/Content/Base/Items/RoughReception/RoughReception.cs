using MSU;
using MSU.Config;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates;

namespace FortunesFromTheScrapyard
{
    public class RoughReception : FFTSItem
    {
        public const string TOKEN = "FFTS_ITEM_ROUGHRECEPTION_DESCRIPTION";

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float swingBaseDamageCoefficient = 2.5f;

        [ConfigureField(FFTSConfig.ID_ITEMS)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float swingDamageCoefficientPerStack = 2.5f;

        public static GameObject roughSwingPrefab;
        public override void Initialize()
        {
            roughSwingPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoSlash.prefab").WaitForCompletion()
                .InstantiateClone("RoughReceptionSwingEffect", false);
            roughSwingPrefab.EnsureComponent<EffectComponent>().applyScale = true;
            roughSwingPrefab.EnsureComponent<DestroyOnTimer>().duration = RoughReceptionComponent.baseSwingDuration / 2f;

            NetworkingAPI.RegisterMessageType<SyncRoughReceptionSwing>();
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig.Invoke(self, skill);

            if (NetworkServer.active && self.HasItem(FFTSContent.Items.RoughReception) && self.skillLocator && skill == self.skillLocator.primary)
            {
                RoughReceptionComponent swingComponent = self.gameObject.EnsureComponent<RoughReceptionComponent>();
                swingComponent.enabled = true;
                swingComponent.RoughReceptionSwing(self);
            }
        }

        private static void PlaySwing(CharacterBody body, float duration, int step)
        {
            CharacterModel model = body.modelLocator && body.modelLocator.modelTransform
                ? body.modelLocator.modelTransform.GetComponent<CharacterModel>()
                : null;
            if (!model) return;

            foreach (GameObject display in model.GetItemDisplayObjects(FFTSContent.Items.RoughReception.itemIndex))
            {
                Animator animator = display ? display.GetComponentInChildren<Animator>() : null;
                if (animator && animator.GetLayerIndex("Body") >= 0)
                {
                    EntityState.PlayAnimationOnAnimator(animator, "Body", "Swing" + (step + 1), "Swing.playbackRate", duration);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override FFTSAssetRequest LoadAssetRequest()
        {
            return FFTSAssets.LoadAssetAsync<ItemAssetCollection>("acRoughReception", FFTSBundle.Items);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            base.ModifyContentPack(contentPack);
            contentPack.effectDefs.AddSingle(new EffectDef(roughSwingPrefab));
        }

        public class RoughReceptionBehaviour : BaseItemBodyBehavior
        {
            private RoughReceptionComponent swingComponent;

            [ItemDefAssociation(useOnClient = false)]
            public static ItemDef GetItemDef() => FFTSContent.Items.RoughReception;
            private void OnEnable()
            {
                swingComponent = body.gameObject.EnsureComponent<RoughReceptionComponent>();
                swingComponent.enabled = true;
            }
            private void OnDisable()
            {
                if (swingComponent) swingComponent.enabled = false;
            }
        }
        public class RoughReceptionComponent : MonoBehaviour
        {
            private CharacterBody body;
            public static float baseSwingDuration = 0.5f;
            private readonly List<float> pendingHits = new List<float>();
            private int step;

            public void RoughReceptionSwing(CharacterBody characterBody)
            {
                body = characterBody;
                float duration = baseSwingDuration / Mathf.Max(characterBody.attackSpeed, 0.01f);
                pendingHits.Add(duration / 2f);

                new SyncRoughReceptionSwing(body.networkIdentity.netId, duration, step).Send(NetworkDestination.Clients);
                step = step == 0 ? 1 : 0;
            }

            public void FixedUpdate()
            {
                if (!NetworkServer.active || !body || !body.healthComponent.alive || !body.HasItem(FFTSContent.Items.RoughReception))
                {
                    pendingHits.Clear();
                    return;
                }

                for (int i = 0; i < pendingHits.Count;)
                {
                    pendingHits[i] -= Time.fixedDeltaTime;
                    if (pendingHits[i] <= 0f)
                    {
                        pendingHits.RemoveAt(i);
                        Fire();
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            private void Fire()
            {
                int itemCount = body.GetItemCount(FFTSContent.Items.RoughReception);
                if (itemCount <= 0 || !body.healthComponent.alive) return;

                Ray aimRay = body.inputBank
                    ? new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection)
                    : new Ray(body.corePosition, body.transform.forward);

                BulletAttack catAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = body.gameObject,
                    weapon = null,
                    bulletCount = 1,
                    damage = body.damage * GetStackValue(swingBaseDamageCoefficient, swingDamageCoefficientPerStack, itemCount),
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = 400f,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0.7f,
                    maxDistance = 12,
                    radius = 5,
                    smartCollision = true,
                    isCrit = body.RollCrit(),
                    muzzleName = "",
                    tracerEffectPrefab = null
                };
                EffectData effectData = new EffectData
                {
                    origin = body.corePosition + aimRay.direction * 2f,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    scale = 1f
                };
                catAttack.Fire();
                EffectManager.SpawnEffect(roughSwingPrefab, effectData, true);
            }

            public void OnDisable()
            {
                pendingHits.Clear();
                body = null;
                step = 0;
            }
        }

        public class SyncRoughReceptionSwing : INetMessage
        {
            private NetworkInstanceId bodyId;
            private float duration;
            private byte step;

            public SyncRoughReceptionSwing()
            {
            }

            public SyncRoughReceptionSwing(NetworkInstanceId bodyId, float duration, int step)
            {
                this.bodyId = bodyId;
                this.duration = duration;
                this.step = (byte)step;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(bodyId);
                writer.Write(duration);
                writer.Write(step);
            }

            public void Deserialize(NetworkReader reader)
            {
                bodyId = reader.ReadNetworkId();
                duration = reader.ReadSingle();
                step = reader.ReadByte();
            }

            public void OnReceived()
            {
                GameObject bodyObject = Util.FindNetworkObject(bodyId);
                CharacterBody body = bodyObject ? bodyObject.GetComponent<CharacterBody>() : null;
                if (!body)
                {
                    FFTSLog.Debug($"Rough Reception swing body {bodyId} is no longer available.");
                    return;
                }

                PlaySwing(body, duration, step);
            }
        }
    }
}
