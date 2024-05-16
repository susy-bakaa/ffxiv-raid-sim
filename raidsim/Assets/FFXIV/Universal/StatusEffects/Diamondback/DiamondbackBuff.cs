using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondbackBuff : StatusEffect
{
    [Header("Function")]
    public float damageReduction = 0.1f;

    public override void OnApplication(CharacterState state)
    {
        if (uniqueTag > 0)
            state.AddDamageReduction(damageReduction, $"{data.statusName}_{uniqueTag}");
        else
            state.AddDamageReduction(damageReduction, data.statusName);
        state.bound = true;
        state.knockbackResistant = true;
        state.canDoActions = false;
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        if (uniqueTag > 0)
            state.RemoveDamageReduction($"{data.statusName}_{uniqueTag}");
        else
            state.RemoveDamageReduction(data.statusName);
        state.bound = false;
        state.knockbackResistant = false;
        state.canDoActions = true;
        base.OnExpire(state);
    }
}
