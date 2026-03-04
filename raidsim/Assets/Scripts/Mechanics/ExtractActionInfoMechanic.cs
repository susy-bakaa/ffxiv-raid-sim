using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ExtractActionInfoMechanic : FightMechanic
    {
        public UnityEvent<CharacterAction> onActionExtracted;
        public UnityEvent<CharacterState> onSourceExtracted;
        public UnityEvent<CharacterState> onTargetExtracted;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (actionInfo.action != null)
                onActionExtracted.Invoke(actionInfo.action);
            if (actionInfo.source != null)
                onSourceExtracted.Invoke(actionInfo.source);
            if (actionInfo.target != null)
                onTargetExtracted.Invoke(actionInfo.target);
        }
    }
}