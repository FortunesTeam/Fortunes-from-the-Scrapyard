using RoR2;
using UnityEngine;

namespace FortunesFromTheScrapyard.Items
{
    public class TakeoutComponent : MonoBehaviour
    {
        private NetworkedBodyAttachment attachment;
        private MeshRenderer[] ringRenderers;
        private MaterialPropertyBlock propertyBlock;
        private BuffDef displayedFood;

        private void Awake()
        {
            attachment = GetComponentInParent<NetworkedBodyAttachment>();
            ringRenderers = attachment.GetComponentsInChildren<MeshRenderer>(true);
            propertyBlock = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            CharacterBody ownerBody = attachment.attachedBody;
            if (!ownerBody)
                return;

            BuffDef nextFood;
            Color color;
            if (ownerBody.HasBuff(FFTSContent.Buffs.bdTakeoutDmg))
            {
                nextFood = FFTSContent.Buffs.bdTakeoutDmg;
                color = Takeout.chickenColor;
            }
            else if (ownerBody.HasBuff(FFTSContent.Buffs.bdTakeoutSpeed))
            {
                nextFood = FFTSContent.Buffs.bdTakeoutSpeed;
                color = Takeout.noodlesColor;
            }
            else if (ownerBody.HasBuff(FFTSContent.Buffs.bdTakeoutRegen))
            {
                nextFood = FFTSContent.Buffs.bdTakeoutRegen;
                color = Takeout.potstickersColor;
            }
            else
            {
                return;
            }

            if (displayedFood == nextFood)
                return;

            displayedFood = nextFood;
            foreach (MeshRenderer renderer in ringRenderers)
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_TintColor", color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
