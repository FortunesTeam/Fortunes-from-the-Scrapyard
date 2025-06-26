using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using static RoR2.CameraTargetParams;
using RoR2;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

namespace EntityStates.Wrecker
{
    public class CrashoutFire : BaseSkillState
    {
        public float baseDuration = 0.25f;

        private float duration;

        private bool hasFired;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            StartAimMode(duration, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasFired && fixedAge >= duration / 2f)
            {
                hasFired = true;
                Fire();
            }
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public void Fire()
        {
            Ray aimRay = GetAimRay();
            if (NetworkServer.active)
            {
                BullseyeSearch search = new BullseyeSearch();
                search.teamMaskFilter = TeamMask.all;
                search.maxAngleFilter = 60f;
                search.maxDistanceFilter = 26f;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.filterByLoS = true;
                search.filterByDistinctEntity = true;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);
                TeamIndex team = GetTeam();
                IEnumerable<HurtBox> hurtBoxes = search.GetResults().Where((hurtBox) => { return Util.IsValid(hurtBox) && FriendlyFireManager.ShouldSplashHitProceed(hurtBox.healthComponent, team); });

                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = gameObject,
                        damage = 6.0f * damageStat,
                        crit = RollCrit(),
                        position = hurtBox.transform.position,
                        procCoefficient = 1f,
                        damageType = DamageType.Stun1s | DamageTypeCombo.GenericPrimary,
                        force = (hurtBox.transform.position - this.transform.position).normalized * 1500f + aimRay.direction.normalized * 1500f,
                        canRejectForce = false,
                    };

                    if ((bool)hurtBox.healthComponent)
                    {
                        hurtBox.healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox.healthComponent.gameObject);
                    }
                }
            }
            if (isAuthority)
            {
                float magnitude = characterMotor.isGrounded ? 2000f : -2500f;
                Vector3 direction = characterMotor.isGrounded ? characterDirection.forward : aimRay.direction;
                characterMotor.ApplyForce(direction * magnitude);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
