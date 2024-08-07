using UnityEngine;
using EntityStates;

using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;

using RoR2.Orbs;
using UnityEngine.Networking;
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using R2API;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public class Sap : BaseNeuromancerSkillState
    {
        public float damageCoefficient = 2.5f;

        public float procCoefficient = 1f;

        public string muzzleString = "Muzzle";

        public GameObject pullTracerPrefab = Neuromancer.captureTracerEffect;

        public string attackSoundString = "Play_huntress_m1_ready";

        public float baseDuration = 0.25f;

        private float duration;

        protected bool isCrit;

        private HurtBox target;

        private ChildLocator childLocator;

        private NeuromancerTracker neuromancerTracker;

        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            Transform modelTransform = GetModelTransform();
            neuromancerTracker = GetComponent<NeuromancerTracker>();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                animator = modelTransform.GetComponent<Animator>();
            } 
            Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
            if (neuromancerTracker && base.isAuthority)
            {
                target = neuromancerTracker.GetTrackingTarget();
            }
            duration = baseDuration / attackSpeedStat;
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(duration + 1f);
            }
            PlayCrossfade("Gesture, Override", "PlayFlurry", "Shoot.playbackRate", duration * 0.4f, duration * 0.2f / attackSpeedStat);
            isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
            FireSiphonOrb();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireSiphonOrb()
        {
            if (NetworkServer.active)
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    position = this.target.transform.position,
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    damage = this.damageCoefficient * base.damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    crit = RollCrit(),
                    force = Vector3.zero,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 1f
                };
                damageInfo.AddModdedDamageType(Neuromancer.DelayedSecondary);

                this.target.healthComponent.TakeDamage(damageInfo);
                GlobalEventManager.instance.OnHitEnemy(damageInfo, this.target.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, this.target.gameObject);

                Vector3 position = this.target.transform.position;
                Vector3 start = base.characterBody.corePosition;
                Transform transform = FindModelChild(muzzleString);
                if (transform)
                {
                    start = transform.position;
                }
                EffectData effectData = new EffectData
                {
                    origin = position,
                    start = start
                };
                EffectManager.SpawnEffect(pullTracerPrefab, effectData, transmit: true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(HurtBoxReference.FromHurtBox(target));
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }

    }
}
