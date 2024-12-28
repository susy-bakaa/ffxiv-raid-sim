using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;
using static GlobalData.Damage;
using static ActionController;
using NaughtyAttributes;

public class GazeMechanic : FightMechanic
{
    public Damage damage = new Damage(100, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.percentageFromMax, "Unnamed Gaze");
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
            target.ModifyHealth(damage, lethalGaze);
            if (log)
                Debug.Log($"Angle was under threshold!");
        }
        else
        {
            target.ShowDamageFlyText(new Damage(0, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, damage.source, damage.name));
            if (log)
                Debug.Log($"Angle was over threshold!");
        }
    }

    private void Reset()
    {
        damage = new Damage(100, true, true, DamageType.unique, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.percentageFromMax, "Unnamed Gaze");
    }
}
