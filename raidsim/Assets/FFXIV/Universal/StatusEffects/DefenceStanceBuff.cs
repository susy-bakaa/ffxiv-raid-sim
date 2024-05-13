using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceStanceBuff : StatusEffect
{
    [Header("Function")]
    public float damageReduction = 0.6f;

    public override void OnApplication(CharacterState state)
    {
        state.damageReduction = damageReduction;
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        state.damageReduction = 1f;
        base.OnExpire(state);
    }
}
