using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

public class TriggerSelectedMechanic : FightMechanic
{
    [Header("Trigger Selected Settings")]
    public List<UnityEvent<ActionInfo>> availableMechanics = new List<UnityEvent<ActionInfo>>();
    public int selectedMechanic = 0;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger())
            return;

        if (availableMechanics != null && availableMechanics.Count > 0)
        {
            if (selectedMechanic >= 0 && selectedMechanic < availableMechanics.Count)
            {
                if (log)
                    Debug.Log($"[FightMechanic.TriggerSelectedMechanic '{mechanicName}'] triggered mechanic {selectedMechanic} out of {availableMechanics.Count}!");
                availableMechanics[selectedMechanic].Invoke(actionInfo);
            }
            else
            {
                Debug.LogError($"[FightMechanic.TriggerSelectedMechanic '{mechanicName}'] Selected mechanic index {selectedMechanic} is out of bounds for available mechanics list.");
            }
        }
    }

    public void SelectMechanic(int value)
    {
        if (value >= availableMechanics.Count)
            value = availableMechanics.Count - 1;
        else if (value < 0)
            value = 0;

        selectedMechanic = value;
    }
}
