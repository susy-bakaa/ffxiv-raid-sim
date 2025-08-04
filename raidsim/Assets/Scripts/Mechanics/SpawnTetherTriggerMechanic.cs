using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SpawnTetherTriggerMechanic : FightMechanic
    {
        [Header("Tether Trigger Settings")]
        public GameObject tetherTriggerPrefab;
        public bool enableInstead = false;
        public Transform startPoint;
        public Vector3 startOffset;
        public Transform spawnLocation;
        public bool setTargetAutomatically = false;
        public float delay = 0f;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (spawnLocation == null)
            {
                if (actionInfo.target != null)
                {
                    GameObject spawned;

                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger for {actionInfo.target.gameObject.name}");
                    if (!enableInstead)
                        spawned = Instantiate(tetherTriggerPrefab, actionInfo.target.transform.position, actionInfo.target.transform.rotation, FightTimeline.Instance.mechanicParent);
                    else
                        spawned = tetherTriggerPrefab;

                    SetupTetherTrigger(spawned, actionInfo);
                }
                else if (actionInfo.source != null && actionInfo.action != null)
                {
                    GameObject spawned;

                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger for {actionInfo.source.gameObject.name}");
                    if (!enableInstead)
                        spawned = Instantiate(tetherTriggerPrefab, actionInfo.source.transform.position, actionInfo.source.transform.rotation, FightTimeline.Instance.mechanicParent);
                    else
                        spawned = tetherTriggerPrefab;

                    SetupTetherTrigger(spawned, actionInfo);
                }
            }
            else
            {
                GameObject spawned;

                if (log)
                    Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger");
                if (!enableInstead)
                    spawned = Instantiate(tetherTriggerPrefab, spawnLocation.position, spawnLocation.rotation, FightTimeline.Instance.mechanicParent);
                else
                    spawned = tetherTriggerPrefab;

                SetupTetherTrigger(spawned, actionInfo);
            }
        }

        public void SetupTetherTrigger(GameObject spawned, ActionInfo actionInfo)
        {
            if (spawned.TryGetComponent(out TetherTrigger tetherTrigger))
            {
                spawned.gameObject.SetActive(true);
                if (startPoint != null)
                {
                    tetherTrigger.startPoint = startPoint;
                    tetherTrigger.startOffset = startOffset;
                }
                if (delay > 0)
                {
                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger with delay of {delay}");

                    spawned.gameObject.SetActive(false);
                    Utilities.FunctionTimer.Create(tetherTrigger, () =>
                    {
                        spawned.gameObject.SetActive(true);
                        if (!tetherTrigger.initializeOnStart)
                        {
                            if (setTargetAutomatically && actionInfo.target != null)
                            {
                                tetherTrigger.Initialize(actionInfo.target);
                            }
                            else
                            {
                                tetherTrigger.Initialize();
                            }
                        }
                    }, delay, $"{tetherTrigger}_{tetherTrigger.GetHashCode()}_{mechanicName.Replace(" ", "")}_Activation_Delay", false, true);
                }
                else if (!tetherTrigger.initializeOnStart)
                {
                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger without any delay");

                    if (setTargetAutomatically && actionInfo.target != null)
                    {
                        tetherTrigger.Initialize(actionInfo.target);
                    }
                    else
                    {
                        tetherTrigger.Initialize();
                    }
                }
            }
        }
    }
}