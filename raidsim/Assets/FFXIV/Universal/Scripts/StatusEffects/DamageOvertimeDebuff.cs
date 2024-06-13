using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalStructs;

public class DamageOvertimeDebuff : StatusEffect
{
    public void Reset()
    {
        damage = new Damage(-1000, true, string.Empty);
    }

    public override void OnTick(CharacterState state)
    {
        state.ModifyHealth(damage);
    }
}
