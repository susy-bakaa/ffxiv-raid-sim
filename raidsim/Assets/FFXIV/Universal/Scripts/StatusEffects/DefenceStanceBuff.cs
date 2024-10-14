using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceStanceBuff : StatusEffect
{
    [Header("Function")]
    public float incomingDamageReduction = 0.6f;
    public float outgoingDamageReduction = 0.6f;
    public float enmityGenerationModifier = 1f;

    public override void OnApplication(CharacterState state)
    {
        if (uniqueTag > 0)
        {
            if (incomingDamageReduction != 1f)
                state.AddDamageReduction(incomingDamageReduction, $"{data.statusName}_{uniqueTag}");
            if (outgoingDamageReduction != 1f)
                state.AddDamageOutputModifier(outgoingDamageReduction, $"{data.statusName}_{uniqueTag}");
            if (enmityGenerationModifier != 1f)
                state.AddEnmityGenerationModifier(enmityGenerationModifier, $"{data.statusName}_{uniqueTag}");
        }
        else
        {
            if (incomingDamageReduction != 1f)
                state.AddDamageReduction(incomingDamageReduction, data.statusName);
            if (outgoingDamageReduction != 1f)
                state.AddDamageOutputModifier(outgoingDamageReduction, data.statusName);
            if (enmityGenerationModifier != 1f)
                state.AddEnmityGenerationModifier(enmityGenerationModifier, data.statusName);
        }
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        if (uniqueTag > 0)
        {
            state.RemoveDamageReduction($"{data.statusName}_{uniqueTag}");
            state.RemoveDamageOutputModifier($"{data.statusName}_{uniqueTag}");
            state.RemoveEnmityGenerationModifier($"{data.statusName}_{uniqueTag}");
        }
        else
        {
            state.RemoveDamageReduction(data.statusName);
            state.RemoveDamageOutputModifier(data.statusName);
            state.RemoveEnmityGenerationModifier(data.statusName);
        }
        base.OnExpire(state);
    }

    public override void OnCleanse(CharacterState state)
    {
        if (uniqueTag > 0)
        {
            state.RemoveDamageReduction($"{data.statusName}_{uniqueTag}");
            state.RemoveDamageOutputModifier($"{data.statusName}_{uniqueTag}");
            state.RemoveEnmityGenerationModifier($"{data.statusName}_{uniqueTag}");
        }
        else
        {
            state.RemoveDamageReduction(data.statusName);
            state.RemoveDamageOutputModifier(data.statusName);
            state.RemoveEnmityGenerationModifier(data.statusName);
        }
        base.OnCleanse(state);
    }
}
