using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace FortunesFromTheScrapyard.Ricochet
{
    public class TrailRendererColorFromEffectData : MonoBehaviour
    {
        private void Start()
        {
            Color color = this.effectComponent.effectData.color;
            for (int i = 0; i < this.renderers.Length; i++)
            {
                this.renderers[i].colorGradient = new Gradient
                {
                    alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f) },
                    colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0f) },
                    mode = GradientMode.Fixed,
                };
            }
        }

        public TrailRenderer[] renderers;
        public EffectComponent effectComponent;
    }
}