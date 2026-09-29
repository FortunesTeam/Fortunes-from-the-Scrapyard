using UnityEngine;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    public class BulletController : MonoBehaviour
    {
        private Rigidbody rb;
        private bool triggered;

        private void Awake()
        {
            this.rb = this.GetComponentInChildren<Rigidbody>();
            this.gameObject.layer = LayerIndex.debris.intVal;
            this.GetComponentInChildren<Collider>().gameObject.layer = LayerIndex.debris.intVal;
            this.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
        }

        private void OnEnable()
        {
            this.triggered = false;
        }

        private void OnCollisionEnter()
        {
            if (!this.triggered)
            {
                this.triggered = true;
                Util.PlaySound("sfx_driver_bullet", this.gameObject);
            }
        }
    }
}