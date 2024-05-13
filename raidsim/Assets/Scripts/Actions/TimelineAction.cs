using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineAction : MonoBehaviour
{
    public ActionController target;
    public TimelineActionData data;

    public void ExecuteAction()
    {
        Debug.Log($"Trying to make character {target} perform action {data.makeCharacterPerformAction.actionName}");

        if (data != null && target != null)
        {
            if (data.makeCharacterPerformAction != null)
            {
                Debug.Log($"Made character {target} perform action {data.makeCharacterPerformAction.actionName}");
                target.PerformAction(data.makeCharacterPerformAction.actionName);
            }
        }
    }
}
