using EntityStates.Bandit2.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Cloaker.Weapon   
{
    public class CloakerChargeShot : BaseSidearmState
    {
        public float duration;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration)
            {
                CloakerShoot shoot = new CloakerShoot
                {
                    baseDamageCoefficient = 4.2f,
                    tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgun.prefab").WaitForCompletion(),
                    hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgun.prefab").WaitForCompletion(),
                    charged = true
                };

                outer.SetNextState(shoot);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}