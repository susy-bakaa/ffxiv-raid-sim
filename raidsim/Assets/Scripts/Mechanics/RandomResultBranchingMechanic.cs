using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class RandomResultBranchingMechanic : FightMechanic
{
    FightTimeline fight;

    public List<RandomResultBranchedEvent> events = new List<RandomResultBranchedEvent>();

    public int randomEventId = 0;
    public bool useResultAsListIndex = true;

    void Start()
    {
        fight = FightTimeline.Instance;
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (useResultAsListIndex && randomEventId >= 0)
        {
            events[fight.GetRandomEventResult(randomEventId)].m_event.Invoke(actionInfo);
            if (log)
                Debug.Log($"fight.GetRandomEventResult(randomEventId) {randomEventId} resulted in {fight.GetRandomEventResult(randomEventId)} -> m_event.Invoke(actionInfo) {events[fight.GetRandomEventResult(randomEventId)].name}");
        }
    }

    [System.Serializable]
    public struct RandomResultBranchedEvent
    {
        public string name;
        public UnityEvent<ActionInfo> m_event;
    }
}
