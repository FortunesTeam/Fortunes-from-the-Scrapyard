using EntityStates;
using RoR2;
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.EntityStates
{
    public abstract class BaseNeuromancerSkillState : BaseSkillState
    {
        protected NeuromancerDrainController drainController;

        protected NeuromancerController neuromancerController;
        public virtual void AddRecoil2(float x1, float x2, float y1, float y2)
        {
            this.AddRecoil(x1, x2, y1, y2);
        }
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected void RefreshState()
        {
            if (!neuromancerController)
            {
                neuromancerController = base.GetComponent<NeuromancerController>();
            }
            if (!drainController)
            {
                drainController = base.GetComponent<NeuromancerDrainController>();
            }
        }
    }
}
