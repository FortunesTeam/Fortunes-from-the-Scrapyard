using EntityStates;
using FortunesFromTheScrapyard.Survivors.Neuromancer;
using EntityStates.Neuromancer;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.UI;
using UnityEngine;


namespace EntityStates.Neuromancer
{
    public class ChargeTimePunch : BaseNeuromancerSkillState
    {
        private class ArcVisualizer : IDisposable
        {
            private readonly Vector3[] points;

            private readonly float duration;

            private readonly GameObject arcVisualizerInstance;

            private readonly LineRenderer lineRenderer;

            public ArcVisualizer(GameObject arcVisualizerPrefab, float duration, int vertexCount)
            {
                arcVisualizerInstance = UnityEngine.Object.Instantiate(arcVisualizerPrefab);
                lineRenderer = arcVisualizerInstance.GetComponent<LineRenderer>();
                lineRenderer.positionCount = vertexCount;
                points = new Vector3[vertexCount];
                this.duration = duration;
            }

            public void Dispose()
            {
                EntityState.Destroy(arcVisualizerInstance);
            }

            public void SetParameters(Vector3 origin, Vector3 initialVelocity, float characterMaxSpeed, float characterAcceleration)
            {
                arcVisualizerInstance.transform.position = origin;
                if (!lineRenderer.useWorldSpace)
                {
                    Vector3 eulerAngles = Quaternion.LookRotation(initialVelocity).eulerAngles;
                    eulerAngles.x = 0f;
                    eulerAngles.z = 0f;
                    Quaternion rotation = Quaternion.Euler(eulerAngles);
                    arcVisualizerInstance.transform.rotation = rotation;
                    origin = Vector3.zero;
                    initialVelocity = Quaternion.Inverse(rotation) * initialVelocity;
                }
                else
                {
                    arcVisualizerInstance.transform.rotation = Quaternion.LookRotation(Vector3.Cross(initialVelocity, Vector3.up));
                }
                float y = Physics.gravity.y;
                float num = duration / (float)points.Length;
                Vector3 vector = origin;
                Vector3 vector2 = initialVelocity;
                float num2 = num;
                float num3 = y * num2;
                float maxDistanceDelta = characterAcceleration * num2;
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = vector;
                    Vector2 current = Util.Vector3XZToVector2XY(vector2);
                    current = Vector2.MoveTowards(current, Vector3.zero, maxDistanceDelta);
                    vector2.x = current.x;
                    vector2.z = current.y;
                    vector2.y += num3;
                    vector += vector2 * num2;
                }
                lineRenderer.SetPositions(points);
            }
        }

        public static GameObject arcVisualizerPrefab;

        public static float arcVisualizerSimulationLength = 2f;

        public static int arcVisualizerVertexCount = 60;

        [SerializeField]
        public float baseChargeDuration = 0.75f;

        public static float minChargeForChargedAttack = 0f;

        public static GameObject chargeVfxPrefab = NeuromancerSurvivor.timePunchChargeEffect;

        public static string chargeVfxChildLocatorName = "HandL";

        public static GameObject crosshairOverridePrefab = NeuromancerSurvivor.chargeCrosshair;

        public static float walkSpeedCoefficient = 0.75f;

        public static string startChargeLoopSFXString = "Play_loader_shift_charge_loop";

        public static string endChargeLoopSFXString = "Stop_loader_shift_charge_loop";

        public static string enterSFXString = "Play_loader_shift_activate";

        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        private Transform chargeVfxInstanceTransform;

        private int gauntlet;

        private uint soundID;

        protected float chargeDuration { get; private set; }

        protected float charge { get; private set; }

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            Util.PlaySound(enterSFXString, base.gameObject);
            soundID = Util.PlaySound(startChargeLoopSFXString, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            if (chargeVfxInstanceTransform)
            {
                EntityState.Destroy(chargeVfxInstanceTransform.gameObject);
                PlayAnimation("Gesture, Additive", "Empty");
                PlayAnimation("Gesture, Override", "Empty");
                crosshairOverrideRequest?.Dispose();
                chargeVfxInstanceTransform = null;
            }
            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            Util.PlaySound(endChargeLoopSFXString, base.gameObject);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            charge = Mathf.Clamp01(base.fixedAge / chargeDuration);
            AkSoundEngine.SetRTPCValueByPlayingID("loaderShift_chargeAmount", charge * 100f, soundID);
            base.characterBody.SetSpreadBloom(charge);
            base.characterBody.SetAimTimer(3f);
            if (charge >= minChargeForChargedAttack && !chargeVfxInstanceTransform && chargeVfxPrefab)
            {
                if (crosshairOverridePrefab && crosshairOverrideRequest == null)
                {
                    crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
                }
                Transform transform = FindModelChild(chargeVfxChildLocatorName);
                if (transform)
                {
                    chargeVfxInstanceTransform = UnityEngine.Object.Instantiate(chargeVfxPrefab, transform).transform;
                    ScaleParticleSystemDuration component = chargeVfxInstanceTransform.GetComponent<ScaleParticleSystemDuration>();
                    if (component)
                    {
                        component.newDuration = (1f - minChargeForChargedAttack) * chargeDuration;
                    }
                }
                PlayCrossfade("Gesture, Additive", "ChargePunchIntro", "ChargePunchIntro.playbackRate", chargeDuration, 0.1f);
                PlayCrossfade("Gesture, Override", "ChargePunchIntro", "ChargePunchIntro.playbackRate", chargeDuration, 0.1f);
            }
            if (chargeVfxInstanceTransform)
            {
                base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
            }
            if (base.isAuthority)
            {
                AuthorityFixedUpdate();
            }
        }

        public override void Update()
        {
            base.Update();
            Mathf.Clamp01(base.age / chargeDuration);
        }

        private void AuthorityFixedUpdate()
        {
            if (!ShouldKeepChargingAuthority())
            {
                outer.SetNextState(GetNextStateAuthority());
            }
        }

        protected virtual bool ShouldKeepChargingAuthority()
        {
            return base.fixedAge < this.chargeDuration; ;
        }

        protected virtual EntityState GetNextStateAuthority()
        {
            return new SwingTimePunch
            {
                charge = this.charge
            };
        }
    }
}
