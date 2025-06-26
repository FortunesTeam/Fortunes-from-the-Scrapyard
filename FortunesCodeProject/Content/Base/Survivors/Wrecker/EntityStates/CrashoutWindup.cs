using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using static RoR2.CameraTargetParams;
using RoR2;
using UnityEngine;

namespace EntityStates.Wrecker
{
    public class CrashoutWindup : BaseSkillState
    {
        public CameraTargetParams.AimRequest aimRequest;
        public float baseDuration = 0.75f;

        private float duration;
        private float halfBonusAttackSpeedStat;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            halfBonusAttackSpeedStat = (attackSpeedStat - 1) * 0.5f + 1;
            healthComponent.AddBarrierAuthority(healthComponent.fullCombinedHealth * 0.2f);
            StartAimMode(duration, false);

            if ((bool)cameraTargetParams)
            {
                aimRequest = GetAimType();
                //cameraTargetParams.RequestAimType(CameraTargetParams.AimType.OverTheShoulder);
            }
        }

        public AimRequest GetAimType()
        {
            CharacterCameraParamsData data3 = cameraTargetParams.cameraParams.data;
            data3.idealLocalCameraPos.value += new Vector3(-2.5f, -0.5f, -1f);
            Debug.LogWarning(data3.fov.value + " FOV | " + data3.fov.alpha + " ALPHA");
            /*data3.fov.value = 40f;
            data3.fov.alpha = 1f;*/
            CameraParamsOverrideHandle overrideHandle = cameraTargetParams.AddParamsOverride(new CameraParamsOverrideRequest
            {
                cameraParamsData = data3,
                priority = 0.1f,
            }, baseDuration / 3f / halfBonusAttackSpeedStat);
            AimRequest aimRequest4 = new AimRequest(AimType.OverTheShoulder, delegate (AimRequest aimRequest)
            {
                cameraTargetParams.RemoveParamsOverride(overrideHandle, baseDuration / 3f / halfBonusAttackSpeedStat);
            });

            return aimRequest4;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = false;
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(new CrashoutFire { activatorSkillSlot = activatorSkillSlot });
                return;
            }
        }

        public override void OnExit()
        {
            aimRequest?.Dispose();

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
