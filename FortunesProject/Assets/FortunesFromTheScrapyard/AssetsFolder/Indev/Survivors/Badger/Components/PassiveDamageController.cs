using UnityEngine;
using RoR2;
using EntityStates.Badger.Components;
using UnityEngine.Networking;

namespace EntityStates.Badger.Components
{
    public class PassiveDamageController : MonoBehaviour
    {
        public CharacterBody attackerBody;
        public CharacterBody characterBody;
        private GameObject victim;
        private CharacterBody victimBody;
        private HealthComponent victimMoveSpeed;
        private void Awake()
        {
            characterBody = this.GetComponent<CharacterBody>();
        }

        private void Start()
        {
        }

        private void FixedUpdate()
        {
            
        }
    }
}