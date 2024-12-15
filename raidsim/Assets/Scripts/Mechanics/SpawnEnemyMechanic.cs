using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static ActionController;
using static PartyList;

public class SpawnEnemyMechanic : FightMechanic
{
    public string enemyObjectName = string.Empty;
    public GameObject enemyObject;
    public bool activateInstead = true;
    public Transform spawnLocation;

    public void Awake()
    {
        if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
        {
            enemyObject = Utilities.FindAnyByName(enemyObjectName);
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (activateInstead && enemyObject == null && !string.IsNullOrEmpty(enemyObjectName))
        {
            enemyObject = Utilities.FindAnyByName(enemyObjectName);
        }

        if (enemyObject == null)
            return;

        if (activateInstead && enemyObject != null)
        {
            //Debug.Log($"Trying to activate {enemyObject.name}");
            if (enemyObject.TryGetComponent(out CharacterState state))
            {
                state.disabled = false;
                //Debug.Log($"Found CharacterState from {enemyObject.name} and setting disabled to {state.disabled}");
            }
            //else
            //{
                //Debug.Log($"Did not find CharacterState from {enemyObject.name} and disabled was not changed!");
            //}
            //Debug.Log($"Before SetActive {enemyObject.name} is active {enemyObject.activeSelf}");
            enemyObject.SetActive(true);
            //Debug.Log($"After SetActive {enemyObject.name} is active {enemyObject.activeSelf}");
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
