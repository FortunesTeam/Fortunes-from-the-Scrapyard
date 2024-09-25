using UnityEngine;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class NeuromancerCSS : MonoBehaviour
    {
        private bool hasPlayed = false;
        private float timer = 0f;
        private void Awake()
        {
        }
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!hasPlayed && timer >= 0.8f)
            {
                hasPlayed = true;
                Util.PlaySound("sfx_neuro_decompress", this.gameObject);
            }
        }
    }
}
