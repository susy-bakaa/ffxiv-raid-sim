using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceStanceBuff : StatusEffect
{
    [Header("Function")]
    public float damageReduction = 0.6f;

    public override void OnApplication(CharacterState state)
    {
        if (uniqueTag > 0)
            state.AddDamageReduction(damageReduction, $"{data.statusName}_{uniqueTag}");
        else
            state.AddDamageReduction(damageReduction, data.statusName);
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        if (uniqueTag > 0)
            state.RemoveDamageReduction($"{data.statusName}_{uniqueTag}");
        else
            state.RemoveDamageReduction(data.statusName);
        base.OnExpire(state);
    }
}
