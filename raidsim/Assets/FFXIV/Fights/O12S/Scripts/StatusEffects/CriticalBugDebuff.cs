using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalBugDebuff : StatusEffect
{
    [Header("Function")]
    public GameObject spawnObjectPrefab;
    public StatusEffectData inflictsStatusEffect;
    public List<StatusEffectData> cleansStatusEffects = new List<StatusEffectData>();

    public override void OnExpire(CharacterState state)
    {
        GameObject spawned = Instantiate(spawnObjectPrefab, state.transform.position, state.transform.rotation, FightTimeline.Instance.mechanicParent);
        if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
        {
            damageTrigger.owner = state;
        }
        state.AddEffect(inflictsStatusEffect);
        if (cleansStatusEffects != null && cleansStatusEffects.Count > 0 )
        {
            for (int i = 0; i < cleansStatusEffects.Count; i++)
            {
                if (state.HasEffect(cleansStatusEffects[i].statusName))
                    state.RemoveEffect(cleansStatusEffects[i].statusName, false);
            }
        }
        base.OnExpire(state);
    }
}
