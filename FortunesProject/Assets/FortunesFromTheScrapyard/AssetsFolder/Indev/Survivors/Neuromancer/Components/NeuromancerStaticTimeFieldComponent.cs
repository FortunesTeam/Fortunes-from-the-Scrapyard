using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class NeuromancerStaticTimeFieldComponent : MonoBehaviour
    {
        private NeuromancerController neuromancerController;
        private CharacterBody ownerBody;

        private void Awake()
        {
        }

        private void Start()
        {
            ownerBody = this.GetComponent<ProjectileController>().owner.GetComponent<CharacterBody>();
            neuromancerController = ownerBody.gameObject.GetComponent<NeuromancerController>();
        }

        private void FixedUpdate()
        {
            if(neuromancerController && ownerBody.healthComponent.alive)
            {
                if(!neuromancerController.drainTimeEssence)
                {
                    UnityEngine.Object.Destroy(base.gameObject);
                }
            }
            else
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }
    }
}
