using EntityStates;

using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class TimeStoppedState : BaseState
    {
        private static float baseMinDuration = 0.2f;
        private float minDuration;
        private TemporaryOverlay temporaryOverlay;
        private Animator modelAnimator;
        public override void OnEnter()
        {
            base.OnEnter();

            CharacterBody body = GetComponent<CharacterBody>();
            if(body) minDuration = body.isBoss || body.isChampion ? baseMinDuration / 2f : baseMinDuration;
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                CharacterModel component = modelTransform.GetComponent<CharacterModel>();
                if (component)
                {
                    this.temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
                    this.temporaryOverlay.duration = 9999f;
                    this.temporaryOverlay.originalMaterial = Neuromancer.frozenMat;
                    this.temporaryOverlay.AddToCharacerModel(component);
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
            if (base.isAuthority && !this.HasBuff(ScrapyardContent.Buffs.bdTimeStopped) && base.fixedAge >= minDuration)
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