using RoR2;
using System.Linq;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerTrackerController : MonoBehaviour
    {
        public float maxTrackingDistance = 60f;

        public float maxTrackingAngle = 10f;

        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;

        private CharacterBody characterBody;

        private TeamComponent teamComponent;

        private InputBankTest inputBank;

        private float trackerUpdateStopwatch;

        private Indicator indicator;

        private readonly BullseyeSearch search = new BullseyeSearch();

        private void Awake()
        {
            indicator = new Indicator(base.gameObject, ScrapyardAssets.GetAssetBundle(ScrapyardBundle.Indev).LoadAsset<GameObject>("CloakerTrackingIndicator"));
        }

        private void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return trackingTarget;
        }

        private void OnEnable()
        {
            indicator.active = true;
        }

        private void OnDisable()
        {
            indicator.active = false;
        }

        private void FixedUpdate()
        {
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
                _ = trackingTarget;
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = trackingTarget ? trackingTarget.transform : null;
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            foreach (HurtBox hurt in this.search.GetResults())
            {
                if (hurt && hurt.healthComponent && hurt.healthComponent.body)
                {
                    if (!hurt.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarkCd) && !hurt.healthComponent.body.HasBuff(ScrapyardContent.Buffs.bdCloakerMarked))
                    {
                        this.search.FilterOutGameObject(hurt.healthComponent.gameObject);
                    }
                }
            }
            trackingTarget = search.GetResults().FirstOrDefault();
        }
    }
}

