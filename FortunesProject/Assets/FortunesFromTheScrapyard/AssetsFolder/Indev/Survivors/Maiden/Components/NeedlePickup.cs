using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FortunesFromTheScrapyard.Survivors.Maiden.Components
{
    public class NeedlePickup : MonoBehaviour
    {
        private float stopwatch;
        public Rigidbody rigidbody;
        public bool collided = false;
        private bool alive = true;
        private SphereCollider sphereCollider;
        private ProjectileController projectileController;
        private Vector3 eulerAngleVelocity;
        private bool stuck;
        private Vector3 contactNormal;
        private Vector3 contactPosition;
        private Vector3 transformPositionWhenContacted;
        private float stopwatchSinceStuck;
        public AnimationCurve embedDistanceCurve;

        private void Awake()
        {
            this.rigidbody = this.GetComponent<Rigidbody>();
            this.sphereCollider = this.GetComponent<SphereCollider>();
            this.sphereCollider.radius = 3f;
            this.sphereCollider.isTrigger = true;
            this.sphereCollider.center = Vector3.zero;
            this.projectileController = base.GetComponent<ProjectileController>();
            if (this.sphereCollider) this.sphereCollider.enabled = false;
            this.stopwatch = 0f;
            this.rigidbody.velocity = Vector3.zero;
            eulerAngleVelocity = new Vector3(15f, 0, 0);
        }


        private void OnTriggerEnter(Collider other)
        {
            CharacterBody characterBody = other.GetComponent<CharacterBody>();
            if (characterBody && characterBody?.bodyIndex == BodyCatalog.FindBodyIndex("MaidenBody"))
            {
                EntityStateMachine spinMachine = null;
                EntityStateMachine[] components = base.gameObject.GetComponents<EntityStateMachine>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i].customName == "Opulance")
                    {
                        spinMachine = components[i];

                        break;
                    }
                }
                if (spinMachine)
                {
                    this.alive = false;
                    //if (Util.HasEffectiveAuthority(spinMachine.networkIdentity)) spinMachine.SetNextState(new OpulantSpin());
                    UnityEngine.Object.Destroy(base.gameObject);
                }
            }
        }
        private void Update()
        {
            if (this.stuck)
            {
                this.stopwatchSinceStuck += Time.deltaTime;
                base.transform.position = this.transformPositionWhenContacted;
            }
        }

        private void FixedUpdate()
        {
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch < 0.5f && alive)
            {
                this.sphereCollider.enabled = false;
                this.rigidbody.velocity = Vector3.up * 15f;
                Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * 1.5f);
                this.rigidbody.MoveRotation(this.rigidbody.rotation * deltaRotation);
            }
            if (this.stopwatch > 0.5f && alive)
            {
                this.sphereCollider.enabled = true;
                this.rigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
                this.rigidbody.rotation = Quaternion.Lerp(this.rigidbody.rotation, Quaternion.identity, this.stopwatch / 1f);
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (this.stuck || this.rigidbody.isKinematic)
            {
                return;
            }
            if (collision.transform.gameObject.layer != LayerIndex.world.intVal)
            {
                return;
            }
            if (stopwatch > 0.5f)
            {
                this.stuck = true;
                ContactPoint contact = collision.GetContact(0);
                this.contactNormal = contact.normal;
                this.contactPosition = contact.point;
                this.transformPositionWhenContacted = base.transform.position;
                this.rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                this.rigidbody.detectCollisions = true;
                this.rigidbody.isKinematic = true;
                this.rigidbody.velocity = Vector3.zero;
            }
        }
    }
}