using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class TriggerRandomMechanic : FightMechanic
{
    public List<FightMechanicList> mechanics = new List<FightMechanicList>();
    [MinValue(-1)]
    public int thisRandomEventResultId = -1;
    [MinValue(-1)]
    public int previousRandomEventResultId = -1;
    [HideIf("chooseBasedOnPreviousResult")] public bool chooseListBasedOnPrevious = false;
    [HideIf("chooseListBasedOnPrevious")] public bool chooseBasedOnPreviousResult = false;
    public bool useIndexMapping = false;
    public List<IndexMapping> indexMapping = new List<IndexMapping>();
#if UNITY_EDITOR
    [Header("Editor")]
    public int editorForcedRandomEventResult = -1;
    public int editorForcedPreviousRandomEventResult = -1;
#endif

    public override void TriggerMechanic(ActionController.ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        int r = -1;

        if (!chooseListBasedOnPrevious && !chooseBasedOnPreviousResult)
        {
            r = UnityEngine.Random.Range(0, mechanics[0].fightMechanics.Count);

#if UNITY_EDITOR
            if (editorForcedRandomEventResult > -1)
                r = editorForcedRandomEventResult;
#endif

            mechanics[0].fightMechanics[r].TriggerMechanic(actionInfo);
        }
        else if (chooseListBasedOnPrevious && !chooseBasedOnPreviousResult && previousRandomEventResultId > -1)
        {
            int p = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId);

#if UNITY_EDITOR
            if (editorForcedPreviousRandomEventResult > -1)
                p = editorForcedPreviousRandomEventResult;
#endif

            if (p <= -1)
                return;

            if (useIndexMapping)
            {
                for (int i = 0; i < indexMapping.Count; i++)
                {
                    if (indexMapping[i].previousIndex == p)
                    {
                        p = indexMapping[i].nextIndex;
                        break;
                    }
                }
            }

            r = UnityEngine.Random.Range(0, mechanics[p].fightMechanics.Count);

#if UNITY_EDITOR
            if (editorForcedRandomEventResult > -1)
                r = editorForcedRandomEventResult;
#endif

            Debug.Log($"r {r} p {p}");

            if (p > -1 && p < mechanics.Count)
            {
                mechanics[p].fightMechanics[r].TriggerMechanic(actionInfo);
            }
        }
        else if (chooseBasedOnPreviousResult && !chooseListBasedOnPrevious && previousRandomEventResultId > -1)
        {
            int b = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId);

#if UNITY_EDITOR
            if (editorForcedPreviousRandomEventResult > -1)
                b = editorForcedPreviousRandomEventResult;
#endif

            if (b <= -1)
                return;

            if (useIndexMapping)
            {
                for (int i = 0; i < indexMapping.Count; i++)
                {
                    if (indexMapping[i].previousIndex == b)
                    {
                        b = indexMapping[i].nextIndex;
                        break;
                    }
                }
            }

            Debug.Log($"b {b}");

            r = b;

            mechanics[0].fightMechanics[r].TriggerMechanic(actionInfo);
        }

        //Debug.Log($"RandomMechanic chosen {r} from total of {mechanics.Count - 1}");

        if (thisRandomEventResultId > -1)
            FightTimeline.Instance.AddRandomEventResult(thisRandomEventResultId, r);
    }

    [System.Serializable]
    public class FightMechanicList
    {
        public string name;
        public List<FightMechanic> fightMechanics = new List<FightMechanic>();
    }

    [System.Serializable]
    public struct IndexMapping
    {
        public string name;
        public int previousIndex;
        public int nextIndex;
    }
}
