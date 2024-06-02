using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSpeedBuff : StatusEffect
{
    [Header("Function")]
    public float newMovementSpeed = 9.45f;
    public float newMovementSpeedModifier = -1f;

    public override void OnApplication(CharacterState state)
    {
        if (uniqueTag != 0)
        {
            if (newMovementSpeed >= 0f)
            {
                state.AddMovementSpeed(newMovementSpeed, $"{data.statusName}_{uniqueTag}");
            }
            if (newMovementSpeedModifier >= 0f)
            {
                state.AddMovementSpeedModifier(newMovementSpeedModifier, $"{data.statusName}_{uniqueTag}");
            }
        }
        else
        {
            if (newMovementSpeed >= 0f)
            {
                state.AddMovementSpeed(newMovementSpeed, data.statusName);
            }
            if (newMovementSpeedModifier >= 0f)
            {
                state.AddMovementSpeedModifier(newMovementSpeedModifier, data.statusName);
            }
        }
    }

    public override void OnExpire(CharacterState state)
    {
        if (uniqueTag != 0)
        {
            if (newMovementSpeed >= 0f)
            {
                state.RemoveMovementSpeed($"{data.statusName}_{uniqueTag}");
            }
            if (newMovementSpeedModifier >= 0f)
            {
                state.RemoveMovementSpeedModifier($"{data.statusName}_{uniqueTag}");
            }
        }
        else
        {
            if (newMovementSpeed >= 0f)
            {
                state.RemoveMovementSpeed(data.statusName);
            }
            if (newMovementSpeedModifier >= 0f)
            {
                state.RemoveMovementSpeedModifier(data.statusName);
            }
        }
        base.OnExpire(state);
    }
}
