// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using UnityEngine;
using UnityEngine.Serialization;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SpawnTetherTriggerMechanic : FightMechanic
    {
        [Header("Tether Trigger Settings")]
        public GameObject tetherTriggerPrefab;
        public bool enableInstead = false;
        public CharacterState tetherSource;
        [Obsolete("Use tetherSource instead. This field is kept for compatability reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public Transform startPoint;
        [FormerlySerializedAs("startOffset")] public Vector3 sourceOffset;
        public Transform spawnLocation;
        public bool setSourceAutomatically = false;
        public bool setTargetAutomatically = false;
        public float delay = 0f;

#if UNITY_EDITOR
#pragma warning disable CS0618 // Disable obsolete warning for startPoint for the editor tool
        [NaughtyAttributes.Button("Migrate")]
        public void MigrateButton()
        {
            if (tetherSource == null && startPoint != null)
            {
                CharacterState character = startPoint.GetComponentInParents<CharacterState>();
                if (character != null)
                {
                    tetherSource = character;
                    Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Migrated startPoint to tetherSource for character {character.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[SpawnTetherTriggerMechanic ({gameObject.name})] Could not find CharacterState component in parent of startPoint {startPoint.gameObject.name}");
                }
            }
        }
#pragma warning restore CS0618
#endif

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
                if (tetherSource != null)
                {
                    tetherTrigger.tetherSource = tetherSource;
                    tetherTrigger.sourceOffset = sourceOffset;
                }
                if (delay > 0)
                {
                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Spawning tether trigger with delay of {delay}");

                    spawned.gameObject.SetActive(false);
                    Utilities.FunctionTimer.Create(tetherTrigger, () =>
                    {
                        spawned.gameObject.SetActive(true);
                        InitializeTether();
                    }, delay, $"{tetherTrigger}_{tetherTrigger.GetHashCode()}_{mechanicName.Replace(" ", "")}_Activation_Delay", false, true);
                }
                else
                {
                    InitializeTether();
                }
            }

            void InitializeTether()
            {
                if (!tetherTrigger.initializeOnStart)
                {
                    if (log)
                        Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Initializing tether trigger\nsetSourceAutomatically '{setSourceAutomatically}' actionInfo.source '{(actionInfo.source != null ? actionInfo.source : "null")}'\nsetTargetAutomatically '{setTargetAutomatically}' actionInfo.target '{(actionInfo.target != null ? actionInfo.target : "null")}'");

                    if (setSourceAutomatically && actionInfo.source != null)
                    {
                        tetherTrigger.tetherSource = actionInfo.source;
                        tetherTrigger.sourceOffset = sourceOffset;
                    }
                    if (setTargetAutomatically && actionInfo.target != null)
                    {
                        if (log)
                            Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Initializing tether trigger with target {actionInfo.target.gameObject.name}");

                        tetherTrigger.Initialize(actionInfo.target);
                    }
                    else
                    {
                        if (log)
                            Debug.Log($"[SpawnTetherTriggerMechanic ({gameObject.name})] Initializing tether trigger without setting target here.\ntetherTrigger '{tetherTrigger.gameObject.name}' tetherTrigger.tetherSource '{(tetherTrigger.tetherSource != null ? ($"{tetherTrigger.tetherSource.characterName} ({tetherTrigger.tetherSource.gameObject.name})") : "null")}' tetherTrigger.tetherTarget '{(tetherTrigger.tetherTarget != null ? ($"{tetherTrigger.tetherTarget.characterName} ({tetherTrigger.tetherTarget.gameObject.name})") : "null")}'");

                        tetherTrigger.Initialize();
                    }
                }
            }
        }
    }
}