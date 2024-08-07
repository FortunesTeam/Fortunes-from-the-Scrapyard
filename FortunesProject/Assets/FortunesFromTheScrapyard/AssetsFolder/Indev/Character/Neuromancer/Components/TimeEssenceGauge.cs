using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using FortunesFromTheScrapyard.Survivors.Neuromancer.Components;


namespace FortunesFromTheScrapyard.Survivors.Neuromancer.Components
{
    public class TimeEssenceGauge : MonoBehaviour
    {
        public HUD targetHUD;
        public NeuromancerController neuromancerController;

        public LanguageTextMeshController targetText;
        public GameObject durationDisplay;
        public Image durationBar;
        public Image durationBarRed;
        public Image durationBarOver;

        private void Start()
        {
            this.neuromancerController = this.targetHUD?.targetBodyObject?.GetComponent<NeuromancerController>();
            this.neuromancerController.onEssenceChange += SetDisplay;

            this.durationDisplay.SetActive(false);
            SetDisplay();
        }

        private void OnDestroy()
        {
            if (this.neuromancerController) this.neuromancerController.onEssenceChange -= SetDisplay;

            this.targetText.token = string.Empty;
            this.durationDisplay.SetActive(false);
        }
        private void FixedUpdate()
        {
            if (neuromancerController.gameObject.GetComponent<CharacterBody>())
            {
                if(!neuromancerController.gameObject.GetComponent<CharacterBody>().healthComponent.alive)
                {
                    Object.Destroy(this);
                }
            }
        }
        private void Update()
        {
            if(targetText.token != string.Empty) { targetText.token = string.Empty; }
            if (this.neuromancerController && this.neuromancerController.currentTimeEssence >= 0f)
            {
                float fill = Util.Remap(this.neuromancerController.currentTimeEssence, 0f, this.neuromancerController.currentTimeEssence, 0f, this.neuromancerController.currentTimeEssence / this.neuromancerController.maxTimeEssence);

                if (this.durationBarRed)
                {
                    this.durationBarRed.fillAmount = Mathf.Lerp(this.durationBarRed.fillAmount, fill, Time.deltaTime * 2f);
                }

                if(this.durationBarOver)
                {
                    this.durationBarOver.fillAmount = Mathf.Max(fill - 1f, 0f);
                }

                this.durationBar.fillAmount = fill;
            }
        }

        private void SetDisplay()
        {
            if (this.neuromancerController)
            {
                this.durationDisplay.SetActive(true);
                this.targetText.token = string.Empty;

                if (this.neuromancerController.currentTimeEssence <=  99f) this.durationBar.color = Neuromancer.lightCyan;
                else this.durationBar.color = Color.red;
            }
            else
            {
                this.durationDisplay.SetActive(false);
            }
        }
    }
}