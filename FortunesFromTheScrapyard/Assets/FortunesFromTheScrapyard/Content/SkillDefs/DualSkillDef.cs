using EntityStates;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;

namespace FortunesFromTheScrapyard.Skills.SkillDefTypes
{
    class DualSkillDef : SkillDef
	{

		#region VERY IMPORTANT REQUIRED NECESSARY
		public override SkillDef.BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
		{
			return new InstanceData
			{
				body = skillSlot.characterBody
			};
		}
		public override void OnUnassigned(GenericSkill skillSlot)
		{
			((InstanceData)skillSlot.skillInstanceData).body = null;
		}
		#endregion
		public SerializableEntityStateType alternateActivationState;
		public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
		{
			EntityState entityState = null;

			DualSkillDef.InstanceData instanceData = (DualSkillDef.InstanceData)skillSlot.skillInstanceData;
			if (instanceData.isPlayerControlled)
			{
				entityState = EntityStateCatalog.InstantiateState(this.activationState);
			}
            else
			{
				entityState = EntityStateCatalog.InstantiateState(this.alternateActivationState);
			}

			ISkillState skillState;
			if ((skillState = (entityState as ISkillState)) != null)
			{
				skillState.activatorSkillSlot = skillSlot;
			}
			return entityState;
		}

		class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public bool isPlayerControlled
            {
                get
                {
					if (body == null)
						return false;
					return body.isPlayerControlled;
                }
            }
			public CharacterBody body
			{
				get
				{
					return this._body;
				}
				set
				{
					if (this._body == value)
					{
						return;
					}
					this._body = value;
				}
			}

			private CharacterBody _body;
		}
	}
}
