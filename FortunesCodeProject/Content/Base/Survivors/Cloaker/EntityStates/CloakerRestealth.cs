using FortunesFromTheScrapyard.Survivors.Cloaker;
using FortunesFromTheScrapyard;
using EntityStates;
using MSU;
using MSU.Config;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;

namespace EntityStates.Cloaker
{
    public class CloakerRestealth : BaseSkillState
    {
        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Dash and invincibility duration in seconds.")]
        [FormatToken(CloakerContent.UTILITYTOKEN, 0)]
        public static float Duration = 0.3f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Dash speed coefficient, scaled by jump power and movement speed.")]
        public static float SpeedCoefficient = 0.3f;

        private Vector3 hop;
        private float speed;
        private CameraTargetParams.AimRequest cameraRequest;
        private bool invincibilityApplied;
        public override void OnEnter()
        {
            base.OnEnter();
            CloakerController controller = GetComponent<CloakerController>();
            characterBody.SetAimTimer(2f);
            if (cameraTargetParams) cameraRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            Vector3 horizontalAim = inputBank.aimDirection;
            horizontalAim.y = 0f;
            Vector3 axis = -Vector3.Cross(Vector3.up, horizontalAim);
            float angle = Vector3.Angle(inputBank.aimDirection, horizontalAim);
            if (inputBank.aimDirection.y < 0f) angle = -angle;
            Vector3 moveDirection = inputBank.moveVector;
            if (moveDirection == Vector3.zero) moveDirection = characterDirection.forward;
            hop = (Quaternion.AngleAxis(angle, axis) * moveDirection).normalized;
            if ((inputBank.aimDirection.y < 0f && Vector3.Angle(horizontalAim, hop) <= 90f)
                || (inputBank.aimDirection.y > 0f && Vector3.Angle(horizontalAim, hop) >= 90f))
                hop.y *= -1f;
            if (Vector3.Angle(inputBank.aimDirection, horizontalAim) <= 45f) hop.y = 0.25f;
            hop.y = Mathf.Clamp(hop.y, 0.1f, 0.75f);
            if (isAuthority)
            {
                characterMotor.velocity = Vector3.zero;
                characterDirection.moveVector = hop;
            }
            PlayCrossfade("FullBody, Override", controller.GetAnimationStateName("FullBody, Override", "Dash"),
                "Utility.playbackRate", Duration, Duration * 0.05f);
            speed = SpeedCoefficient * characterBody.jumpPower * Mathf.Clamp(characterBody.moveSpeed / 4f, 5f, 20f);
            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                invincibilityApplied = true;
                if (!characterBody.hasCloakBuff && !controller.isAkimbo)
                {
                    characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                    EffectManager.SimpleSoundEffect(CloakerAssets.StealthSound.index, characterBody.corePosition, true);
                }
            }
            if (!controller.isAkimbo) controller.StartGracePeriod();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && characterMotor && characterDirection)
            {
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = hop * speed;
            }
            if (isAuthority && fixedAge >= Duration) outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            cameraRequest?.Dispose();
            if (NetworkServer.active && invincibilityApplied)
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Frozen;
    }
}
