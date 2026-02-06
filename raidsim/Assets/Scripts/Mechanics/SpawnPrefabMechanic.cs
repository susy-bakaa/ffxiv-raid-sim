// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SpawnPrefabMechanic : FightMechanic
    {
        [Header("Spawn Prefab Settings")]
        public GameObject objectPrefab;
        public Transform spawnLocation;
        public Transform lookTarget;
        public Axis rotationAxis;
        public bool enableInstead = false;
        public string spawnEnemyName = string.Empty;
        public bool triggerOnSpawn = false;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (spawnLocation == null)
            {
                if (actionInfo.target != null)
                {
                    GameObject spawned;

                    if (!enableInstead)
                    {
                        spawned = Instantiate(objectPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);
                    }
                    else
                    {
                        spawned = objectPrefab;
                        spawned.SetActive(true);
                        spawned.transform.position = actionInfo.target.transform.position;
                        spawned.transform.rotation = actionInfo.target.transform.rotation;
                        spawned.transform.SetParent(FightTimeline.Instance.mechanicParent);
                    }

                    HandleSpawnedObject(spawned, actionInfo);
                }
                else if (actionInfo.source != null && actionInfo.action != null)
                {
                    GameObject spawned;

                    if (!enableInstead)
                    {
                        spawned = Instantiate(objectPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);
                    }
                    else
                    {
                        spawned = objectPrefab;
                        spawned.SetActive(true);
                        spawned.transform.position = actionInfo.source.transform.position;
                        spawned.transform.rotation = actionInfo.source.transform.rotation;
                        spawned.transform.SetParent(FightTimeline.Instance.mechanicParent);
                    }

                    HandleSpawnedObject(spawned, actionInfo);
                }
            }
            else
            {
                GameObject spawned;

                if (!enableInstead)
                {
                    spawned = Instantiate(objectPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);
                }
                else
                {
                    spawned = objectPrefab;
                    spawned.SetActive(true);
                    spawned.transform.position = spawnLocation.position;
                    spawned.transform.rotation = spawnLocation.rotation;
                    spawned.transform.SetParent(FightTimeline.Instance.mechanicParent);
                }

                HandleSpawnedObject(spawned, actionInfo);
            }
        }
#pragma warning disable CS0618 // Suppress warnings for using deprecated SpawnEnemyMechanic, since this method relies on it for compatibility
        private void HandleSpawnedObject(GameObject spawned, ActionInfo actionInfo)
        {
            if (lookTarget != null)
            {
                Vector3 rotation = spawned.transform.eulerAngles;
                spawned.transform.LookAt(lookTarget);
                spawned.transform.eulerAngles = new Vector3(rotationAxis.x ? spawned.transform.eulerAngles.x : rotation.x, rotationAxis.y ? spawned.transform.eulerAngles.y : rotation.y, rotationAxis.z ? spawned.transform.eulerAngles.z : rotation.z);

            }

            if (!string.IsNullOrEmpty(spawnEnemyName) && spawned.TryGetComponent(out SpawnEnemyMechanic enemyMechanic))
            {
                enemyMechanic.enemyObjectName = spawnEnemyName;
                if (enemyMechanic.enemyObject != null && enemyMechanic.enemyObject.name != spawnEnemyName)
                {
                    enemyMechanic.enemyObject = null;
                }
            }

            if (triggerOnSpawn && spawned.TryGetComponent(out AutomaticMechanic auto))
            {
                if (!auto.onEnable && !auto.onStart)
                {
                    auto.TriggerMechanic(actionInfo);
                }
            }
        }
#pragma warning restore CS0618 // Restore warnings for using deprecated SpawnEnemyMechanic
    }
}