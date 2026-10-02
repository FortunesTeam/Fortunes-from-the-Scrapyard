using EntityStates;
using RoR2;
using RoR2.Skills;

namespace FortunesFromTheScrapyard.Survivors.Cloaker.Components
{
    [UnityEngine.CreateAssetMenu(menuName = "FortunesFromTheScrapyard/SkillDefs/CloakerShootSkillDef")]
    public class CloakerShootSkillDef : SkillDef
    {
        public int stepCount = 2;
        public float stepGraceDuration = 0.1f;
        private class InstanceData : BaseSkillInstanceData
        {
            internal int step;
            internal float resetTimer;
        }
        public override BaseSkillInstanceData OnAssigned(GenericSkill slot) => new InstanceData();
        public override EntityState InstantiateNextState(GenericSkill slot)
        {
            EntityState state = base.InstantiateNextState(slot);
            if (state is SteppedSkillDef.IStepSetter stepped) stepped.SetStep(((InstanceData)slot.skillInstanceData).step);
            return state;
        }
        public override void OnExecute(GenericSkill slot)
        {
            base.OnExecute(slot);
            InstanceData data = (InstanceData)slot.skillInstanceData;
            data.step = (data.step + 1) % stepCount;
        }
        public override void OnFixedUpdate(GenericSkill slot, float deltaTime)
        {
            base.OnFixedUpdate(slot, deltaTime);
            InstanceData data = (InstanceData)slot.skillInstanceData;
            data.resetTimer = slot.CanExecute() ? data.resetTimer + deltaTime : 0f;
            if (data.resetTimer > stepGraceDuration) data.step = 0;
        }
    }

}
