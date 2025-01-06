using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using static GlobalData;
using static GlobalData.Damage;
using static StatusEffectData;

public class GazeMechanic : FightMechanic
{
    public Damage damage = new Damage(100, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.percentageFromMax, "Unnamed Gaze");
    public List<StatusEffectInfo> inflictsEffects = new List<StatusEffectInfo>();
    public bool lethalGaze = true;
    public Transform origin;
    public float threshold = 45f;

    [ShowNonSerializedField] private float angle;
    private CharacterState sourceCharacter;

    private void Awake()
    {
        if (origin != null)
            sourceCharacter = origin.GetComponent<CharacterState>();
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (origin == null)
        {
            if (actionInfo.target != null && actionInfo.source != null)
                origin = actionInfo.source.transform;
            else
                origin = transform;
        }
        if (sourceCharacter == null && origin != null)
        {
            sourceCharacter = origin.GetComponent<CharacterState>();
        }
        if (sourceCharacter != null)
        {
            damage.source = sourceCharacter;
        }

        CharacterState target = actionInfo.target ?? actionInfo.source;
        if (target == null)
            return;

        angle = Vector3.Angle(target.transform.forward, origin.position - target.transform.position);

        if (log)
            Debug.Log($"target: '{target.gameObject.name}', Angle: '{angle}'");

        if (angle < threshold)
        {
            bool damageDealt = false;
            if (damage.value != 0 || lethalGaze)
            {
                damageDealt = true;
                target.ModifyHealth(damage, lethalGaze);
            }
            if (inflictsEffects != null && inflictsEffects.Count > 0)
            {
                if (!damageDealt)
                    target.ShowDamageFlyText(new Damage(-1, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, damage.source, damage.name), false);

                for (int i = 0; i < inflictsEffects.Count; i++)
                {
                    target.AddEffect(inflictsEffects[i].data, sourceCharacter, false, inflictsEffects[i].tag, inflictsEffects[i].stacks);
                }
            }
            if (log)
                Debug.Log($"Angle was under threshold!");
        }
        else
        {
            target.ShowDamageFlyText(new Damage(0, true, true, DamageType.none, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, damage.source, damage.name));
            if (log)
                Debug.Log($"Angle was over threshold!");
        }
    }

    private void Reset()
    {
        damage = new Damage(100, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.percentageFromMax, "Unnamed Gaze");
    }
}
