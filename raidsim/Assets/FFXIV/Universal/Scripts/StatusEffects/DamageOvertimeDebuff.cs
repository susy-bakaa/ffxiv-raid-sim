using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalStructs;

public class DamageOvertimeDebuff : StatusEffect
{
    [Header("Function")]
    public Damage damage = new Damage(-1000, true);

    public override void OnTick(CharacterState state)
    {
        state.ModifyHealth(damage);
    }
}
