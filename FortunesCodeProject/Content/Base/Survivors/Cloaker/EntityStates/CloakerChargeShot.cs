using FortunesFromTheScrapyard.Survivors.Cloaker;
using EntityStates;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Cloaker.Weapon
{
    public class CloakerChargeShot : BaseSkillState
    {
        public const float MaxDamageCoefficient = 4f;
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
            duration = 1.5f / attackSpeedStat;
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
            if (isAuthority && ((!IsKeyDownAuthority() && fixedAge >= 0.5f) || fixedAge >= duration))
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
