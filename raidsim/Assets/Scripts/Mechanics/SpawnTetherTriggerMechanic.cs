using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static GlobalStructs.Damage;
using static GlobalStructs;

public class SpawnTetherTriggerMechanic : FightMechanic
{
    public GameObject tetherTriggerPrefab;
    public Transform startPoint;
    public Vector3 startOffset;
    public Transform spawnLocation;
    public float delay = 0f;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (spawnLocation == null)
        {
            if (actionInfo.target != null)
            {
                GameObject spawned = Instantiate(tetherTriggerPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);

                SetupTetherTrigger(spawned, actionInfo);
            }
            else if (actionInfo.source != null && actionInfo.action != null)
            {
                GameObject spawned = Instantiate(tetherTriggerPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);

                SetupTetherTrigger(spawned, actionInfo);
            }
        }
        else
        {
            GameObject spawned = Instantiate(tetherTriggerPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);

            SetupTetherTrigger(spawned, actionInfo);
        }
    }

    public void SetupTetherTrigger(GameObject spawned, ActionInfo actionInfo)
    {
        if (spawned.TryGetComponent(out TetherTrigger tetherTrigger))
        {
            if (startPoint != null)
            {
                tetherTrigger.startPoint = startPoint;
                tetherTrigger.startOffset = startOffset;
            }
            if (delay > 0)
            {
                spawned.gameObject.SetActive(false);
                Utilities.FunctionTimer.Create(() =>
                {
                    spawned.gameObject.SetActive(true);
                    if (!tetherTrigger.initializeOnStart)
                        tetherTrigger.Initialize();
                }, delay, $"{tetherTrigger}_{tetherTrigger.GetHashCode()}_{mechanicName.Replace(" ", "")}_Activation_Delay", false, true);
            }
            else if (!tetherTrigger.initializeOnStart)
            {
                tetherTrigger.Initialize();
            }
        }
    }
}
