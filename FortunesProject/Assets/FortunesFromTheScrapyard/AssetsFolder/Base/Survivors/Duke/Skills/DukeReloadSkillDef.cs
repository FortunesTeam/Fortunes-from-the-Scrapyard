using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using EntityStates;

namespace FortunesFromTheScrapyard.Survivors.Duke.Skills
{
    [CreateAssetMenu(menuName = "FortunesFromTheScrapyard/SkillDefs/DukeReloadSkillDef")]
    public class DukeReloadSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public int currentStock;

            public float graceStopwatch;
        }

        [Tooltip("The reload state to go into, when stock is less than max.")]
        [Header("Reload Parameters")]
        public SerializableEntityStateType reloadState;

        [Tooltip("The priority of this reload state.")]
        public InterruptPriority reloadInterruptPriority = InterruptPriority.Skill;

        [Tooltip("The amount of time to wait between when we COULD reload, and when we actually start")]
        public float graceDuration;

        [Header("Icons")]
        [Tooltip("The main icon")]
        public Sprite primaryIcon;
        [Tooltip("The icon when empowered")]
        public Sprite secondaryIcon;

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
            instanceData.currentStock = skillSlot.stock;
            if (skillSlot.characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot) || (IsReady(skillSlot) && (bool)skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState()))
            {
                return skillSlot.stateMachine.CanInterruptState(interruptPriority);
            }

            return false;
        }
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData();
        }
        public override void OnUnassigned([NotNull] GenericSkill skillSlot)
        {
            base.OnUnassigned(skillSlot);
        }

        public override Sprite GetCurrentIcon([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
            instanceData.currentStock = skillSlot.stock;
            if (instanceData.currentStock == 1 || skillSlot.characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
            {
                return secondaryIcon;
            }
            else if (instanceData.currentStock > 1 || instanceData.currentStock < 1)
            {
                return primaryIcon;
            }

            return base.GetCurrentIcon(skillSlot);
        }
        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            base.OnFixedUpdate(skillSlot, deltaTime);
            InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
            instanceData.currentStock = skillSlot.stock;

            if (instanceData.currentStock >= GetMaxStock(skillSlot))
            {
                return;
            }

            if (skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState() && skillSlot.stateMachine.CanInterruptState(reloadInterruptPriority) && !skillSlot.characterBody.HasBuff(ScrapyardContent.Buffs.bdDukeFreeShot))
            {
                instanceData.graceStopwatch += Time.fixedDeltaTime;
                if (instanceData.graceStopwatch >= graceDuration || instanceData.currentStock == 0)
                {
                    skillSlot.stateMachine.SetNextState(EntityStateCatalog.InstantiateState(ref reloadState));
                }
            }
            else
            {
                instanceData.graceStopwatch = 0f;
            }
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            ((InstanceData)skillSlot.skillInstanceData).currentStock = skillSlot.stock;
            GetCurrentIcon(skillSlot);
        }
    }

}
