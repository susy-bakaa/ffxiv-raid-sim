using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SetFightTimescaleMechanic : FightMechanic
    {
        public float newTimeScale = 1f;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            FightTimeline.timeScale = newTimeScale;
        }
    }
}