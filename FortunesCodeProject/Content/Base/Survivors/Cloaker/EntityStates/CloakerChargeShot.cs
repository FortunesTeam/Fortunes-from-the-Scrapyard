using FortunesFromTheScrapyard.Survivors.Cloaker;
using FortunesFromTheScrapyard;
using EntityStates;
using MSU;
using MSU.Config;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using CloakerContent = FortunesFromTheScrapyard.Survivors.Cloaker.Cloaker;

namespace EntityStates.Cloaker.Weapon
{
    public class CloakerChargeShot : BaseSkillState
    {
        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Fully charged shot damage coefficient. 4 deals 400% damage.")]
        [FormatToken(CloakerContent.SECONDARYTOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float MaxDamageCoefficient = 4f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Full charge time in seconds before attack speed scaling.")]
        [FormatToken(CloakerContent.SECONDARYTOKEN, 1)]
        public static float BaseDuration = 1.5f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Minimum seconds before releasing a partially charged shot. A full charge can fire sooner.")]
        public static float MinimumChargeDuration = 0.5f;

        private float duration;
        private float? cloakedDamage;
        private CrosshairUtils.OverrideRequest crosshair;
        private uint chargeSoundId;
        private CloakerController controller;
        public override void OnEnter()
        {
            bool captureStealthDamage = isAuthority && !cloakedDamage.HasValue && characterBody.hasCloakBuff;
            if (captureStealthDamage && characterBody.statsDirty)
                characterBody.RecalculateStats();
            base.OnEnter();
            if (captureStealthDamage)
                cloakedDamage = damageStat;
            duration = BaseDuration / attackSpeedStat;
            controller = GetComponent<CloakerController>();
            controller.BreakStealth();
            PlayCrossfade("Gesture, Override", controller.GetAnimationStateName("Gesture, Override", "EnterSecondary"),
                "Secondary.playbackRate", duration, duration * 0.05f);
            chargeSoundId = Util.PlaySound(CloakerAssets.ChargeSound, gameObject);
            crosshair = CrosshairUtils.RequestOverrideForBody(characterBody, CloakerAssets.ChargeCrosshair, CrosshairUtils.OverridePriority.Skill);
            StartAimMode(duration + 2f);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && ((!IsKeyDownAuthority() && fixedAge >= MinimumChargeDuration) || fixedAge >= duration))
                outer.SetNextState(new CloakerShoot
                {
                    damageCoefficient = MaxDamageCoefficient * Mathf.Clamp01(fixedAge / duration),
                    charged = true,
                    cloakedDamage = cloakedDamage
                });
        }
        public override void Update()
        {
            base.Update();
            characterBody.SetSpreadBloom(0f);
        }
        public override void OnExit()
        {
            if (chargeSoundId != 0)
            {
                AkSoundEngine.StopPlayingID(chargeSoundId);
                chargeSoundId = 0;
            }
            crosshair?.Dispose();
            cloakedDamage = null;
            if (!outer.destroying)
                PlayAnimation("Gesture, Override", controller.GetAnimationStateName("Gesture, Override", "BufferEmpty"));
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;

        public override void Reset()
        {
            base.Reset();
            cloakedDamage = null;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(cloakedDamage.HasValue);
            writer.Write(cloakedDamage.GetValueOrDefault());
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            bool hasCloakedDamage = reader.ReadBoolean();
            float damage = reader.ReadSingle();
            cloakedDamage = hasCloakedDamage ? (float?)damage : null;
        }
    }
}
