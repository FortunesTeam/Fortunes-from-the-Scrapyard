using EntityStates;
using RoR2;
using UnityEngine;
using FortunesFromTheScrapyard.Survivors.Skater;
using FortunesFromTheScrapyard;

namespace EntityStates.Skater
{
    public class BubbledState : BaseState
    {
        private TemporaryOverlay temporaryOverlay;
        private Animator modelAnimator;
        private bool hasBounced;
        public override void OnEnter()
        {
            base.OnEnter();

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                CharacterModel component = modelTransform.GetComponent<CharacterModel>();
                if (component)
                {
                    TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(base.gameObject);
                    temporaryOverlayInstance.duration = 2.5f;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = SkaterSurvivor.tempBubbledMat;
                    temporaryOverlayInstance.inspectorCharacterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                    temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlayInstance.animateShaderAlpha = true;
                }
            }
            this.modelAnimator = base.GetModelAnimator();
            if (this.modelAnimator)
            {
                this.modelAnimator.enabled = false;
            }
            if (base.rigidbody && !base.rigidbody.isKinematic)
            {
                base.rigidbody.velocity = Vector3.zero;
                if (base.rigidbodyMotor)
                {
                    base.rigidbodyMotor.moveVector = Vector3.zero;
                }
            }

            if (base.characterDirection)
            {
                base.characterDirection.moveVector = base.characterDirection.forward;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (hasBounced) base.characterMotor.velocity.y = 0f;
            else if (base.fixedAge >= 0.75f)
            {
                hasBounced = true;
            }
            if (base.isAuthority && !this.HasBuff(ScrapyardContent.Buffs.bdSkaterBubbleBuff))
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (this.modelAnimator)
            {
                this.modelAnimator.enabled = true;
            }
            if (this.temporaryOverlay)
            {
                EntityState.Destroy(this.temporaryOverlay);
            }
            base.OnExit();
        }
    }
}
