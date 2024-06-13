using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class FightMechanic : MonoBehaviour
{
    public string mechanicName = string.Empty;

    public void TriggerMechanic(CharacterState state)
    {
        TriggerMechanic(new ActionInfo(null, state, null));
    }

    public virtual void TriggerMechanic(ActionInfo actionInfo)
    {
        Debug.Log("Base FightMechanic.TriggerMechanic(ActionInfo action) called.");
    }
}
