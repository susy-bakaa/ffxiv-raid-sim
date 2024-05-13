using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBleedDebuff : StatusEffect
{
    [Header("Function")]
    public int damage = -1000;

    public override void OnTick(CharacterState state)
    {
        state.ModifyHealth(damage);
    }
}
