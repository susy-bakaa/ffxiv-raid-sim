using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;

public class SpawnEnemyMechanic : FightMechanic
{
    public string enemyObjectName = string.Empty;
    public GameObject enemyObject;
    public bool activateInstead = true;
    public Transform spawnLocation;

    void Awake()
    {
        if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
        {
            enemyObject = GameObject.Find(enemyObjectName);
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
        {
            enemyObject = GameObject.Find(enemyObjectName);
        }

        if (enemyObject == null)
            return;

        if (activateInstead && enemyObject != null)
        {
            enemyObject.SetActive(true);
            if (enemyObject.TryGetComponent(out CharacterState state))
            {
                state.disabled = false;
            }
        }
        else if (enemyObject != null)
        {
            if (spawnLocation == null)
            {
                if (actionInfo.target != null)
                {
                    GameObject spawned = Instantiate(enemyObject, actionInfo.target.transform.position, actionInfo.target.transform.rotation, GameObject.Find("Enemies").transform);
                    if (spawned.TryGetComponent(out CharacterState state))
                    {
                        state.disabled = false;
                    }
                }
                else if (actionInfo.source != null && actionInfo.action != null)
                {
                    GameObject spawned = Instantiate(enemyObject, actionInfo.source.transform.position, actionInfo.source.transform.rotation, GameObject.Find("Enemies").transform);
                    if (spawned.TryGetComponent(out CharacterState state))
                    {
                        state.disabled = false;
                    }
                }
            }
            else
            {
                GameObject spawned = Instantiate(enemyObject, spawnLocation.position, spawnLocation.rotation, GameObject.Find("Enemies").transform);
                if (spawned.TryGetComponent(out CharacterState state))
                {
                    state.disabled = false;
                }
            }
        }
    }
}
