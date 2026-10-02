using FortunesFromTheScrapyard.Survivors.Cloaker;
using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Cloaker
{
    public class CloakerMark : BaseSkillState
    {
        private HurtBox victim;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = 0.5f / attackSpeedStat;
            CloakerTrackerController tracker = GetComponent<CloakerTrackerController>();
            if (isAuthority) victim = tracker.GetTrackingTarget();
            if (!victim || !victim.healthComponent || !victim.healthComponent.body)
            {
                if (isAuthority) outer.SetNextStateToMain();
                return;
            }
            StartAimMode(duration);
            string animation = GetComponent<CloakerController>().GetAnimationStateName("Gesture, Override", "Special2");
            PlayCrossfade("Gesture, Override", animation, "Special.playbackRate", duration, duration * 0.05f);
            Util.PlaySound(CloakerAssets.PingSound, gameObject);
            if (NetworkServer.active && tracker.IsValidTarget(victim))
                victim.healthComponent.body.AddBuff(FortunesFromTheScrapyard.FFTSContent.Buffs.bdCloakerMarked);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(victim));
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            victim = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
    }
}
