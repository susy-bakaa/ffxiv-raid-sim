using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class FightMechanic : MonoBehaviour
{
    public virtual void TriggerMechanic(ActionInfo action)
    {
        Debug.Log("Base FightMechanic.TriggerMechanic(ActionInfo action) called.");
    }
}
