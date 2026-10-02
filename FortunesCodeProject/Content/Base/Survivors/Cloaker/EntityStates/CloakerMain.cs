using FortunesFromTheScrapyard.Survivors.Cloaker;

namespace EntityStates.Cloaker
{
    public class CloakerMain : GenericCharacterMain
    {
        private CloakerController controller;
        private bool previouslyGrounded;

        public override void OnEnter()
        {
            base.OnEnter();
            controller = GetComponent<CloakerController>();
            previouslyGrounded = isGrounded;
            if (hasModelAnimator)
            {
                string animation = isGrounded || !hasCharacterMotor ? "Idle" : "AscendDescend";
                modelAnimator.CrossFadeInFixedTime(controller.GetAnimationStateName("Body", animation),
                    0.1f, modelAnimator.GetLayerIndex("Body"));
                modelAnimator.Update(0f);
            }
        }

        public override void ProcessJump()
        {
            int jumpCount = hasCharacterMotor ? characterMotor.jumpCount : 0;
            base.ProcessJump();
            if (hasModelAnimator && hasCharacterMotor && characterMotor.jumpCount > jumpCount)
                modelAnimator.CrossFadeInFixedTime(controller.GetAnimationStateName("Body", "Jump"),
                    smoothingParameters.intoJumpTransitionTime, modelAnimator.GetLayerIndex("Body"));
        }

        public override void FixedUpdate()
        {
            bool landed = hasCharacterMotor && isGrounded && !previouslyGrounded;
            previouslyGrounded = isGrounded;
            base.FixedUpdate();
            if (hasModelAnimator && landed)
                modelAnimator.PlayInFixedTime("Impact.LightImpact", modelAnimator.GetLayerIndex("Impact"), 0f);
        }
    }
}
