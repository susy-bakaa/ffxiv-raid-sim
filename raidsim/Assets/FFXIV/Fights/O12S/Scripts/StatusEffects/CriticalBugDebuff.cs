using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalBugDebuff : StatusEffect
{
    [Header("Function")]
    public GameObject spawnsAoe;
    public StatusEffectData inflictsStatusEffect;

    public override void OnExpire(CharacterState state)
    {
        GameObject spawned = Instantiate(spawnsAoe, state.transform.position, state.transform.rotation, GameObject.Find("Mechanics").transform);
        state.AddEffect(inflictsStatusEffect);
        base.OnExpire(state);
    }
}
