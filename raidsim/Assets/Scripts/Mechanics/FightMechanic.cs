using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class FightMechanic : MonoBehaviour
{
    public string mechanicName = string.Empty;

    public virtual void TriggerMechanic(ActionInfo action)
    {
        Debug.Log("Base FightMechanic.TriggerMechanic(ActionInfo action) called.");
    }
}
