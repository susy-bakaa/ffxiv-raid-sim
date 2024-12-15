using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ActionController;

public class AutomaticMechanic : FightMechanic
{
    public bool onStart;
    public bool onEnable;
    public GameObject target;
    public UnityEvent<ActionInfo> onTrigger;

    private bool _triggered = false;

    private void Start()
    {
        _triggered = false;

        if (onStart)
            TriggerMechanic(new ActionInfo(null, null, null));
    }

    private void Update()
    {
        if (onEnable && target != null && !_triggered)
        {
            if (target.scene.isLoaded && target.activeSelf)
            {
                TriggerMechanic(new ActionInfo(null, null, null));
            }
        } 
        else if (onEnable && target != null && _triggered)
        {
            if (target.scene.isLoaded && !target.activeSelf)
            {
                _triggered = false;
            }
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        _triggered = true;
        onTrigger.Invoke(actionInfo);
    }
}
