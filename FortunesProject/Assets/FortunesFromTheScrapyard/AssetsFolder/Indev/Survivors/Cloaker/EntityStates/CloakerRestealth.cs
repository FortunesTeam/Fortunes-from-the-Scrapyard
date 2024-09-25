using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;
using FortunesFromTheScrapyard.Survivors.Cloaker;

namespace EntityStates.Cloaker
{
    public class CloakerRestealth : BaseSkillState
    {
        protected Vector3 hopVector;
        public float duration = 0.3f;
        public float speedCoefficient = 7f;
        protected CameraTargetParams.AimRequest request;
        private CloakerController cloakerController;

        public override void OnEnter()
        {
            cloakerController = base.gameObject.GetComponent<CloakerController>();

            base.OnEnter();

            characterBody.SetAimTimer(2f);

            if (cameraTargetParams)
            {
                request = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            hopVector = GetHopVector();

            characterMotor.velocity = Vector3.zero;
            Vector3 to = inputBank.aimDirection;
            to.y = 0f;
            if (inputBank.aimDirection.y < 0f && (Vector3.Angle(to, hopVector) <= 90) || inputBank.aimDirection.y > 0f && (Vector3.Angle(to, hopVector) >= 90))
            {
                hopVector.y *= -1;
            }
            if (Vector3.Angle(inputBank.aimDirection, to) <= 45)
            {
                hopVector.y = 0.25f;
            }

            hopVector.y = Mathf.Clamp(hopVector.y, 0.1f, 0.75f);

            characterDirection.moveVector = hopVector;

            base.PlayCrossfade("FullBody, Override", "Roll", "Dash.playbackRate", this.duration * 1.5f, 0.05f);
            base.PlayAnimation("Gesture, Override", "BufferEmpty");

            speedCoefficient = 0.3f * characterBody.jumpPower * Mathf.Clamp((characterBody.moveSpeed) / 4f, 5f, 20f);

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                if (!characterBody.hasCloakBuff && !cloakerController.isAkimbo) characterBody.AddBuff(RoR2Content.Buffs.Cloak);
            }

            if(!cloakerController.isAkimbo)
            {
                cloakerController.passiveCloakOn = true;
                cloakerController.StartGracePeriod();
            }

            Util.PlaySound("sfx_driver_air_dodge", this.gameObject);
            
        }
        protected virtual Vector3 GetHopVector()
        {
            Vector3 aimDirection = inputBank.aimDirection;
            aimDirection.y = 0f;
            Vector3 axis = -Vector3.Cross(Vector3.up, aimDirection);
            float num = Vector3.Angle(inputBank.aimDirection, aimDirection);
            if (inputBank.aimDirection.y < 0f)
            {
                num = 0f - num;
            }
            return Vector3.Normalize(Quaternion.AngleAxis(num, axis) * inputBank.moveVector);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (characterMotor && characterDirection && base.isAuthority)
            {
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = hopVector * speedCoefficient;
            }

            if (fixedAge >= this.duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (!outer.destroying)
            {
                if (cameraTargetParams)
                {
                    request.Dispose();
                }
            }
            base.OnExit();

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
