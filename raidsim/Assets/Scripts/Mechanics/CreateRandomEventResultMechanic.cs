using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CreateRandomEventResultMechanic : FightMechanic
    {
        [Header("Create Random Event Result")]
        [Label("ID")] public int id = 0;
        public bool pickBetweenMaxAndMin = false;
        [ShowIf("pickBetweenMaxAndMin")] public int minResult = 0;
        [ShowIf("pickBetweenMaxAndMin")] public int maxResult = 1;
        public UnityEvent<ActionInfo> onFinished;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (minResult > maxResult)
            {
                minResult = maxResult - 1;
            }
            else if (maxResult < minResult)
            {
                maxResult = minResult + 1;
            }
        }
#endif

        private void Awake()
        {
            if (minResult > maxResult)
            {
                minResult = maxResult - 1;
            }
            else if (maxResult < minResult)
            {
                maxResult = minResult + 1;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (FightTimeline.Instance == null)
                return;

            if (id >= 0)
            {
                if (pickBetweenMaxAndMin)
                {
                    FightTimeline.Instance.SetRandomEventResult(id, Random.Range(minResult, maxResult + 1));
                }
            }

            onFinished?.Invoke(actionInfo);
        }
    }
}