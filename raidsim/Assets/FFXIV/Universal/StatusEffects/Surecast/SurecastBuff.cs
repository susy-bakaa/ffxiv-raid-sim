using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurecastBuff : StatusEffect
{
    public override void OnApplication(CharacterState state)
    {
        state.knockbackResistant = true;
    }

    public override void OnExpire(CharacterState state)
    {
        state.knockbackResistant = false;
        base.OnExpire(state);
    }
}
