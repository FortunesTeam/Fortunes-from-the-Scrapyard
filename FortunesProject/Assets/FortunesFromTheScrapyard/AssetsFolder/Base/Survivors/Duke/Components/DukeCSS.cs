using UnityEngine;
using RoR2;

namespace FortunesFromTheScrapyard.Survivors.Duke.Components
{
    public class DukeCSS : MonoBehaviour
    {
        private bool hasPlayed = false;
        private float timer = 0f;
        private void Awake()
        {
        }
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!hasPlayed && timer >= 3.5f)
            {
                hasPlayed = true;
            }
        }
    }
}
