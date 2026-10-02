using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Cloaker
{
    public class CloakerController : MonoBehaviour
    {
        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Out-of-combat delay before passive stealth, in seconds. Reduced by cooldown reduction.")]
        [FormatToken(Cloaker.PASSIVETOKEN, 0)]
        public static float BaseRestealthCooldown = 7f;

        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Seconds Restealth protects Cloak from damage and proximity reveals.")]
        [FormatToken(Cloaker.UTILITYTOKEN, 1)]
        public static float BaseGracePeriod = 3f;

        private const float DefaultDetectionRadius = 6f;
        [ConfigureField(FFTSConfig.ID_SURVIVORS, configDescOverride = "Enemy proximity reveal radius in meters.")]
        [FormatToken(Cloaker.PASSIVETOKEN, 2)]
        public static float DetectionRadius = DefaultDetectionRadius;

        public SkillDef passiveCloakSkillDef;
        public SkillDef passiveAkimboSkillDef;
        public SkillDef akimboPrimarySkillDef;
        public GenericSkill passiveSkillSlot;
        public GameObject mainhandModel;
        public GameObject offhandModel;
        public Animator modelAnimator;

        public bool isAkimbo => passiveSkillSlot && passiveSkillSlot.skillDef == passiveAkimboSkillDef;
        public bool isCloak => passiveSkillSlot && passiveSkillSlot.skillDef == passiveCloakSkillDef;
        public float graceTimer { get; private set; }
        public bool passiveCloakOn { get; private set; }

        private CharacterBody body;
        private SkillLocator skills;
        private bool initialized;
        private bool appliedAkimbo;
        private bool emoting;
        private float restealthTimer;
        private GameObject indicator;
        private Transform radius;
        private Collider[] proximityHits = new Collider[128];

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            skills = GetComponent<SkillLocator>();
        }

        private void Start() => RefreshPassive();

        private void RefreshPassive()
        {
            if (initialized && appliedAkimbo == isAkimbo) return;
            initialized = true;
            appliedAkimbo = isAkimbo;
            UpdateWeaponVisibility();
            modelAnimator.SetBool("isAkimbo", appliedAkimbo);
            if (appliedAkimbo)
                skills.primary.SetSkillOverride(this, akimboPrimarySkillDef, GenericSkill.SkillOverridePriority.Upgrade);
            else
                skills.primary.UnsetSkillOverride(this, akimboPrimarySkillDef, GenericSkill.SkillOverridePriority.Upgrade);
            body.MarkAllStatsDirty();
            if (appliedAkimbo) BreakStealth();
        }

        public string GetAnimationStateName(string layerName, string stateName)
            => $"{layerName}.{(isAkimbo ? "Akimbo" : "Default")}.{stateName}";

        public void SetEmoting(bool value)
        {
            emoting = value;
            UpdateWeaponVisibility();
        }

        private void UpdateWeaponVisibility()
        {
            if (mainhandModel) mainhandModel.SetActive(!emoting);
            if (offhandModel) offhandModel.SetActive(!emoting && isAkimbo);
        }

        public void StartGracePeriod()
        {
            graceTimer = BaseGracePeriod;
            passiveCloakOn = true;
        }

        public void BreakStealth()
        {
            if (NetworkServer.active)
            {
                if (body.HasBuff(RoR2Content.Buffs.Cloak)) body.RemoveBuff(RoR2Content.Buffs.Cloak);
                if (body.HasBuff(RoR2Content.Buffs.CloakSpeed)) body.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
            }
            passiveCloakOn = false;
        }

        private void FixedUpdate()
        {
            RefreshPassive();
            graceTimer = Mathf.Max(0f, graceTimer - Time.fixedDeltaTime);
            if (NetworkServer.active && body.healthComponent && body.healthComponent.alive && !isAkimbo)
            {
                if (body.outOfCombat)
                {
                    restealthTimer += Time.fixedDeltaTime;
                    float cooldown = Mathf.Min(BaseRestealthCooldown,
                        Mathf.Max(0.5f, BaseRestealthCooldown * skills.primary.cooldownScale - skills.primary.flatCooldownReduction));
                    if (restealthTimer >= cooldown && !body.hasCloakBuff && !passiveCloakOn)
                    {
                        restealthTimer = 0f;
                        passiveCloakOn = true;
                        body.AddBuff(RoR2Content.Buffs.Cloak);
                        body.AddBuff(RoR2Content.Buffs.CloakSpeed);
                        EffectManager.SimpleSoundEffect(CloakerAssets.StealthSound.index, body.corePosition, true);
                    }
                }
                if (passiveCloakOn && body.hasCloakBuff && graceTimer <= 0f) CheckProximity();
                if (!body.hasCloakBuff) passiveCloakOn = false;
            }

            bool show = NetworkClient.active && !isAkimbo && body.healthComponent && body.healthComponent.alive && body.hasCloakBuff;
            if (show && !indicator)
            {
                indicator = Instantiate(CloakerAssets.RangeIndicator, body.corePosition, Quaternion.identity);
                radius = indicator.transform.Find("Radius");
            }
            if (indicator)
            {
                indicator.SetActive(show);
                indicator.transform.position = body.corePosition;
                indicator.transform.localScale = Vector3.one * (DetectionRadius / DefaultDetectionRadius);
                radius.gameObject.SetActive(show && graceTimer <= 0f);
            }
        }

        private void CheckProximity()
        {
            int count = Physics.OverlapSphereNonAlloc(body.corePosition, DetectionRadius, proximityHits,
                LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Collide);
            while (count == proximityHits.Length)
            {
                System.Array.Resize(ref proximityHits, proximityHits.Length * 2);
                count = Physics.OverlapSphereNonAlloc(body.corePosition, DetectionRadius, proximityHits,
                    LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Collide);
            }
            for (int i = 0; i < count; ++i)
            {
                Collider collider = proximityHits[i];
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                CharacterBody other = hurtBox && hurtBox.healthComponent
                    ? hurtBox.healthComponent.body : collider.GetComponentInParent<CharacterBody>();
                if (!other || other == body || !other.healthComponent || !other.healthComponent.alive || !other.teamComponent) continue;
                if (other.teamComponent.teamIndex == body.teamComponent.teamIndex) continue;
                BreakStealth();
                break;
            }
            System.Array.Clear(proximityHits, 0, count);
        }

        private void OnDisable()
        {
            SetEmoting(false);
            if (indicator) Destroy(indicator);
            indicator = null;
        }
    }
}
