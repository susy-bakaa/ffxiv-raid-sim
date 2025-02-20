using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SetRandomEventResult : MonoBehaviour
{
    [Label("ID"), MinValue(0)]
    public int id = 0;
    [MinValue(0)]
    public int result = 0;

    public void SetResult(int result)
    {
        this.result = result;
        SetResult();
    }

    public void SetResult()
    {
        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.SetRandomEventResult(id, result);
        }
    }
}
