using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintBuff : StatusEffect
{
    [Header("Function")]
    public float newMovementSpeed = 9.45f;

    public override void OnApplication(CharacterState state)
    {
        state.speed = newMovementSpeed;
    }

    public override void OnExpire(CharacterState state)
    {
        state.speed = state.defaultSpeed;
        base.OnExpire(state);
    }
}
