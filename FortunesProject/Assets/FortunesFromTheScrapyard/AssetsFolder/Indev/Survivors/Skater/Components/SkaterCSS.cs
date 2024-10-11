using UnityEngine;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Skater.Components
{
    public class SkaterCSS : MonoBehaviour
    {
        private bool hasPlayed = false;
        private bool hasPlayed2 = false;
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
            }

            if (!hasPlayed2 && timer >= 1.25f)
            {
                hasPlayed2 = true;
            }
        }
    }
}
