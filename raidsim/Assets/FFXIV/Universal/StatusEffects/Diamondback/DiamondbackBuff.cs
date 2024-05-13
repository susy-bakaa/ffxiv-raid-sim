using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondbackBuff : StatusEffect
{
    [Header("Function")]
    public float damageReduction = 0.1f;

    public override void OnApplication(CharacterState state)
    {
        state.damageReduction = damageReduction;
        state.bound = true;
        state.knockbackResistant = true;
        state.canDoActions = false;
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        state.damageReduction = 1f;
        state.bound = false;
        state.knockbackResistant = false;
        state.canDoActions = true;
        base.OnExpire(state);
    }
}
