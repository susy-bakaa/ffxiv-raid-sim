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
    public bool chooseListBasedOnPrevious = false;
    public bool useIndexMapping = false;
    public List<IndexMapping> indexMapping = new List<IndexMapping>();

    public override void TriggerMechanic(ActionController.ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        int r = -1;

        if (!chooseListBasedOnPrevious)
        {
            r = UnityEngine.Random.Range(0, mechanics[0].fightMechanics.Count);

            mechanics[0].fightMechanics[r].TriggerMechanic(actionInfo);
        }
        else if (previousRandomEventResultId > -1)
        {
            int p = FightTimeline.Instance.GetRandomEventResult(previousRandomEventResultId);

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

            if (p > -1 && p < mechanics.Count)
            {
                mechanics[p].fightMechanics[r].TriggerMechanic(actionInfo);
            }
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
