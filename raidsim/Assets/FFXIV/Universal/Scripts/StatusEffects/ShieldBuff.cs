using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterState;

public class ShieldBuff : StatusEffect
{
    public Shield shield;

    void Awake()
    {
        if (!string.IsNullOrEmpty(damage.name) && damage.value != 0)
            shield = new Shield(damage.name, damage.value);
    }

    public override void OnApplication(CharacterState state)
    {
        state.AddShield(shield.value, shield.key);
        base.OnApplication(state);
    }

    public override void OnExpire(CharacterState state)
    {
        state.RemoveShield(shield.key);
        base.OnExpire(state);
    }

    public override void OnCleanse(CharacterState state)
    {
        state.RemoveShield(shield.key);
        base.OnCleanse(state);
    }
}
