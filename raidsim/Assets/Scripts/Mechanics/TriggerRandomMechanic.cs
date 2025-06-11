using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static GlobalData;

public class TriggerRandomMechanic : FightMechanic
{
    [Header("Trigger Random Mechanic Settings")]
    public List<FightMechanicList> mechanics = new List<FightMechanicList>();
    [MinValue(-1)]
    public int thisRandomEventResultId = -1;
    [MinValue(-1)]
    public int previousRandomEventResultId = -1;
    public bool useMultipleResults = false;
    [ShowIf("showSecondId")][MinValue(-1)] public int previousRandomEventResultId2 = -1;
    [HideIf("chooseBasedOnPreviousResult")] public bool chooseListBasedOnPrevious = false;
    [HideIf("chooseListBasedOnPrevious")] public bool chooseBasedOnPreviousResult = false;
    [ShowIf("showBased2")] public bool chooseBasedOnPreviousResult2 = false;
    public bool useIndexMapping = false;
    public List<IndexMapping> indexMapping = new List<IndexMapping>();

    [Header("Editor")]
    public int editorForcedRandomEventResult = -1;
    public int editorForcedPreviousRandomEventResult = -1;

    private bool showSecondId => (useMultipleResults && previousRandomEventResultId >= 0 && !chooseBasedOnPreviousResult);
    private bool showBased2 => (showSecondId && previousRandomEventResultId2 >= 0 && chooseListBasedOnPrevious);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!useMultipleResults)
        {
            previousRandomEventResultId2 = -1;
            chooseBasedOnPreviousResult2 = false;
        }

        if (chooseBasedOnPreviousResult)
        {
            previousRandomEventResultId2 = -1;
            chooseListBasedOnPrevious = false;
            chooseBasedOnPreviousResult2 = false;
        }
        if (chooseListBasedOnPrevious)
        {
            chooseBasedOnPreviousResult = false;
        }
    }
#endif

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        int r = -1;

        if (!chooseListBasedOnPrevious && !chooseBasedOnPreviousResult)
        {
            r = UnityEngine.Random.Range(0, mechanics[0].fightMechanics.Count);

            if (editorForcedRandomEventResult > -1)
                r = editorForcedRandomEventResult;

            if (log)
                Debug.Log($"[{gameObject.name}] r {r}");

            mechanics[0].fightMechanics[r].TriggerMechanic(actionInfo);
        }
        else if (chooseListBasedOnPrevious && !chooseBasedOnPreviousResult && previousRandomEventResultId > -1)
        {
            int p = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId);

            if (editorForcedPreviousRandomEventResult > -1)
                p = editorForcedPreviousRandomEventResult;

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

            if (!useMultipleResults)
            {
                r = Random.Range(0, mechanics[p].fightMechanics.Count);
            }
            else if (useMultipleResults && previousRandomEventResultId2 > -1 && chooseBasedOnPreviousResult2)
            {
                r = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId2);
            }

            if (editorForcedRandomEventResult > -1)
                r = editorForcedRandomEventResult;

            if (log)
                Debug.Log($"[{gameObject.name}] r {r} p {p}");

            if (p > -1 && p < mechanics.Count)
            {
                mechanics[p].fightMechanics[r].TriggerMechanic(actionInfo);
            }
        }
        else if (chooseBasedOnPreviousResult && !chooseListBasedOnPrevious && previousRandomEventResultId > -1)
        {
            int b = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId);

            if (editorForcedPreviousRandomEventResult > -1)
                b = editorForcedPreviousRandomEventResult;

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

            if (log)
                Debug.Log($"[{gameObject.name}] b {b}");

            r = b;

            mechanics[0].fightMechanics[r].TriggerMechanic(actionInfo);
        }

        //Debug.Log($"RandomMechanic chosen {r} from total of {mechanics.Count - 1}");

        if (thisRandomEventResultId > -1)
            FightTimeline.Instance.AddRandomEventResult(thisRandomEventResultId, r);
    }

    public void SetEditorRandomEventResult(int result)
    {
        editorForcedRandomEventResult = result;
    }

    public void SetEditorPreviousRandomEventResult(int result)
    {
        editorForcedPreviousRandomEventResult = result;
    }

    public void ResetEditorRandomEventResults()
    {
        editorForcedRandomEventResult = -1;
        editorForcedPreviousRandomEventResult = -1;
    }

    [System.Serializable]
    public class FightMechanicList
    {
        public string name;
        public List<FightMechanic> fightMechanics = new List<FightMechanic>();
    }
}
