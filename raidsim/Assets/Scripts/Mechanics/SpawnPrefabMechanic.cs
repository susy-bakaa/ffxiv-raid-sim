using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;

public class SpawnPrefabMechanic : FightMechanic
{
    [Header("Spawn Prefab Settings")]
    public GameObject objectPrefab;
    public Transform spawnLocation;
    public string spawnEnemyName = string.Empty;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (spawnLocation == null)
        {
            if (actionInfo.target != null)
            {
                GameObject spawned = Instantiate(objectPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);

                HandleSpawnedObject(spawned);
            }
            else if (actionInfo.source != null && actionInfo.action != null)
            {
                GameObject spawned = Instantiate(objectPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);

                HandleSpawnedObject(spawned);
            }
        }
        else
        {
            GameObject spawned = Instantiate(objectPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);

            HandleSpawnedObject(spawned);
        }
    }

    private void HandleSpawnedObject(GameObject spawned)
    {
        if (!string.IsNullOrEmpty(spawnEnemyName) && spawned.TryGetComponent(out SpawnEnemyMechanic enemyMechanic))
        {
            enemyMechanic.enemyObjectName = spawnEnemyName;
            if (enemyMechanic.enemyObject != null && enemyMechanic.enemyObject.name != spawnEnemyName)
            {
                enemyMechanic.enemyObject = null;
            }
        }
    }
}
