using EntityStates;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using FortunesFromTheScrapyard;
using FortunesFromTheScrapyard.Survivors.Duke.Components;

namespace EntityStates.Duke
{
    public class Flourish : BaseSkillState
    {
        private float baseDuration = 0.15f;
        public GameObject selfOnHitOverlayEffectPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercDashHitOverlay.prefab").WaitForCompletion();
        public float speedCoefficient = 10f;

        private bool hasHit;
        private Vector3 dashVelocity => dashVector * moveSpeedStat * speedCoefficient;

        private Vector3 dashVector;

        private Transform modelTransform;

        private Quaternion slideRotation;

        private int originalLayer;

        public override void OnEnter()
        {
            base.OnEnter();

            if(NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }
            Util.PlaySound("Play_merc_shift_start", this.gameObject);

            Animator anim = this.GetModelAnimator();

            modelTransform = GetModelTransform();

            this.dashVector = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;

            originalLayer = base.gameObject.layer;
            base.gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(base.teamComponent.teamIndex).intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = Vector3.zero;

            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.dashVector;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
            float num = Vector3.Dot(this.dashVector, rhs);
            float num2 = Vector3.Dot(this.dashVector, rhs2);
            anim.SetFloat("dashF", num);
            anim.SetFloat("dashR", num2);
            this.slideRotation = Quaternion.LookRotation(this.dashVector, this.characterDirection.forward);

            base.PlayCrossfade("FullBody, Override", "Dash", 0.1f);

            if (BrotherMonster.BaseSlideState.slideEffectPrefab && base.characterBody)
            {
                Vector3 position = base.characterBody.corePosition;
                Quaternion rotation = Quaternion.identity;
                Transform transform = base.FindModelChild("Base");

                if (transform)
                {
                    position = transform.position;
                }

                if (base.characterDirection)
                {
                    rotation = Util.QuaternionSafeLookRotation(this.slideRotation * base.characterDirection.forward, Vector3.up);
                }

                EffectManager.SimpleEffect(EntityStates.BrotherMonster.BaseSlideState.slideEffectPrefab, position, rotation, false);
            }

            if (modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlayInstance.duration = 0.7f;
                temporaryOverlayInstance.animateShaderAlpha = true;
                temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlayInstance.destroyComponentOnEnd = true;
                temporaryOverlayInstance.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEnergized");
                temporaryOverlayInstance.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
            }

            base.characterDirection.forward = base.characterMotor.velocity.normalized;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor.rootMotion += dashVelocity * Time.fixedDeltaTime;
            base.characterBody.isSprinting = true;

            if(base.isAuthority && base.fixedAge >= this.baseDuration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (skillLocator.primary.stock == 0)
            {
                skillLocator.primary.stock++;
            }
            else if (NetworkServer.active)
            {
                if (base.characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
                {
                    base.characterBody.ClearTimedBuffs(ScrapyardContent.Buffs.bdDukeFreeShot);
                }
                base.characterBody.AddTimedBuff(ScrapyardContent.Buffs.bdDukeFreeShot, 5f);
            }

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }

            PlayAnimation("FullBody, Override", "BufferEmpty");

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            Util.PlaySound("Play_merc_shift_end", base.gameObject);

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
