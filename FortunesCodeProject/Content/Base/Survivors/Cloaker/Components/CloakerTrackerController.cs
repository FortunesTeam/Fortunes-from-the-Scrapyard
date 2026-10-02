using MSU;
using MSU.Config;
using RoR2;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerTrackerController : MonoBehaviour
    {
        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Maximum Mark targeting distance in meters.")]
        [FormatToken(Cloaker.MARKTOKEN, 0)]
        public static float MaxTrackingDistance = 40f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Maximum Mark targeting angle in degrees.")]
        public static float MaxTrackingAngle = 10f;

        public RoR2.Skills.SkillDef markSkillDef;
        private readonly BullseyeSearch search = new BullseyeSearch();
        private CharacterBody body;
        private InputBankTest input;
        private SkillLocator skills;
        private GenericSkill observedSpecial;
        private Indicator indicator;
        private HurtBox target;
        private float stopwatch;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            input = GetComponent<InputBankTest>();
            skills = GetComponent<SkillLocator>();
            indicator = new Indicator(gameObject, CloakerAssets.TrackingIndicator);
        }
        private void OnEnable() => UpdateTrackingState();
        private void OnDisable() { StopObservingSpecial(); ClearTracking(); }
        private void OnDestroy() { StopObservingSpecial(); ClearTracking(); }
        public HurtBox GetTrackingTarget() => UpdateTrackingState() && IsValidTarget(target) ? target : null;

        private bool UpdateTrackingState()
        {
            if (!isActiveAndEnabled)
            {
                ClearTracking();
                return false;
            }
            GenericSkill special = skills ? skills.special : null;
            if (!object.ReferenceEquals(observedSpecial, special))
            {
                StopObservingSpecial();
                observedSpecial = special;
                if (observedSpecial) observedSpecial.onSkillChanged += OnSpecialChanged;
                ClearTracking();
            }
            bool equipped = special && markSkillDef && special.skillDef == markSkillDef
                && body && body.healthComponent && body.healthComponent.alive;
            if (!equipped)
            {
                ClearTracking();
                return false;
            }
            indicator.active = true;
            return true;
        }

        private void OnSpecialChanged(GenericSkill special)
        {
            ClearTracking();
            UpdateTrackingState();
        }

        private void StopObservingSpecial()
        {
            if (!object.ReferenceEquals(observedSpecial, null)) observedSpecial.onSkillChanged -= OnSpecialChanged;
            observedSpecial = null;
        }

        private void ClearTracking()
        {
            target = null;
            stopwatch = 0f;
            if (indicator == null) return;
            indicator.targetTransform = null;
            indicator.SetVisible(false);
            indicator.active = false;
        }

        internal bool IsValidTarget(HurtBox candidate)
        {
            if (!candidate || !candidate.healthComponent || !candidate.healthComponent.alive) return false;
            CharacterBody other = candidate.healthComponent.body;
            return other && other != body && other.teamComponent
                && TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex).HasTeam(other.teamComponent.teamIndex)
                && !other.HasBuff(FFTSContent.Buffs.bdCloakerMarked) && !other.HasBuff(FFTSContent.Buffs.bdCloakerMarkCd)
                && (candidate.transform.position - input.aimOrigin).sqrMagnitude <= MaxTrackingDistance * MaxTrackingDistance;
        }

        private void FixedUpdate()
        {
            if (!UpdateTrackingState()) return;
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch > 0f) return;
            stopwatch = 0.1f;
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = input.aimOrigin;
            search.searchDirection = input.aimDirection;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = MaxTrackingDistance;
            search.maxAngleFilter = MaxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);
            target = null;
            foreach (HurtBox candidate in search.GetResults())
                if (IsValidTarget(candidate)) { target = candidate; break; }
            indicator.targetTransform = target ? target.transform : null;
        }
    }
}
