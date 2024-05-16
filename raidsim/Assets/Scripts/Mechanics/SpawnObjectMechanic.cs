using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class SpawnObjectMechanic : FightMechanic
{
    public GameObject spawn;
    public Transform location;
    public bool whiteWind = false;
    public float multiplier = 1f;

    public override void TriggerMechanic(ActionInfo action)
    {
        if (location == null)
        {
            if (action.target != null)
            {
                GameObject spawned = Instantiate(spawn, action.target.transform.position, action.target.transform.rotation, GameObject.Find("Mechanics").transform);
                
                if (whiteWind && action.source != null)
                {
                    if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
                    {
                        damageTrigger.damage = Mathf.RoundToInt(multiplier * action.source.health);
                    }
                }
            }
            else if (action.source != null)
            {
                GameObject spawned = Instantiate(spawn, action.source.transform.position, action.source.transform.rotation, GameObject.Find("Mechanics").transform);

                if (whiteWind)
                {
                    if (spawned.TryGetComponent(out DamageTrigger damageTrigger))
                    {
                        damageTrigger.damage = Mathf.RoundToInt(multiplier * action.source.health);
                    }
                }
            }
        }
        else
        {
            Instantiate(spawn, location.position, location.rotation, GameObject.Find("Mechanics").transform);
        }
    }
}
