// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
using System.Collections.Generic;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using Random = UnityEngine.Random;

namespace dev.susybaka.raidsim.Mechanics
{
    public class MoveObjectsMechanic : FightMechanic, ISerializationCallbackReceiver
    {
        private List<Animator> targetAnimators;

        [Header("Move Object Settings")]
        [HideInInspector, Obsolete("Use targetsList instead.")] public Transform[] targets;
        public List<Transform> targetsList = new List<Transform>();
        [HideIf("multipleDestinations")] public Transform destination;
        [HideInInspector, Obsolete("Use destinationsList instead.")] public Transform[] destinations;
        [ShowIf("multipleDestinations")] public List<Transform> destinationsList = new List<Transform>();
        public bool multipleDestinations = false;
        public bool relative = false;
        public bool local = false;
        public bool allowDynamicTargets = false;
        public bool randomizeDestinations = false;
        public bool assignStatusEffectsToRandomizedTargets = false;
        [ShowIf(nameof(assignStatusEffectsToRandomizedTargets))] public List<StatusEffectInfo> effectsToAssign;
        public Vector3 destinationPosition;
        public bool rotate = false;
        [ShowIf(nameof(rotate))] public Vector3 destinationRotation;
        public bool copyTargetRotation = false;
        public bool faceTarget = false;
        [ShowIf(nameof(faceTarget))] public bool multipleRotationTargets = false;
        [ShowIf(nameof(_showSingleRotationTarget))] public Transform rotationTarget;
        [ShowIf(nameof(multipleRotationTargets))] public List<Transform> rotationTargetsList = new List<Transform>();
        public float animationDuration = -1f;
        public string triggerAnimation = string.Empty;
        public bool playDirectly = false;
        public bool saveEachAsRandomEventResult = false;
        [ShowIf(nameof(saveEachAsRandomEventResult))] public int baseRandomEventResultId = -1;

        private int triggerAnimationHash;
        Coroutine ieMoveObjectDelayed;
        List<Transform> originalTargetsList = new List<Transform>();

        bool _showSingleRotationTarget => faceTarget && !multipleRotationTargets;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!faceTarget)
            {
                multipleRotationTargets = false;
            }
            else
            {
                if (multipleRotationTargets)
                {
                    rotationTarget = null;
                }
                else
                {
                    rotationTargetsList.Clear();
                }
            }
        }
#endif

        private void Start()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (targets != null && targets.Length > 0)
            {
                targetsList.AddRange(targets);
                targets = new Transform[0];
                targets = null;
            }
            if (destinations != null && destinations.Length > 0)
            {
                destinationsList.AddRange(destinations);
                targets = new Transform[0];
                destinations = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            triggerAnimationHash = Animator.StringToHash(triggerAnimation);

            if (multipleDestinations)
            {
                destination = null;

                if (destinationsList == null || targetsList == null)
                {
                    Debug.LogError("Multiple destinations selected but targets or destinations are missing!");
                    return;
                }
                if (destinationsList.Count != targetsList.Count)
                {
                    Debug.LogError("Multiple destinations selected but the number of targets and destinations do not match!");
                    return;
                }
            }

            if (targetsList != null)
            {
                targetAnimators = new List<Animator>();
                for (int i = 0; i < targetsList.Count; i++)
                {
                    targetAnimators.Add(targetsList[i].GetComponentInChildren<Animator>());
                }
            }

            originalTargetsList = new List<Transform>(targetsList);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if (targets != null && targets.Length > 0)
            {
                targetsList.AddRange(targets);
                targets = new Transform[0];
                targets = null;
            }
            if (destinations != null && destinations.Length > 0)
            {
                destinationsList.AddRange(destinations);
                targets = new Transform[0];
                destinations = null;
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (targetAnimators != null && !string.IsNullOrEmpty(triggerAnimation))
            {
                for (int i = 0; i < targetAnimators.Count; i++)
                {
                    if (playDirectly)
                    {
                        targetAnimators[i].CrossFadeInFixedTime(triggerAnimationHash, 0.2f);
                    }
                    else
                    {
                        targetAnimators[i].SetTrigger(triggerAnimationHash);
                    }
                }
            }

            if (animationDuration > 0)
            {
                if (ieMoveObjectDelayed == null)
                    ieMoveObjectDelayed = StartCoroutine(IE_MoveObjectDelayed(new WaitForSeconds(animationDuration)));
            }
            else
            {
                MoveObjects();
            }
        }

        // Interrupt does nothing for this mechanic, but it is called on timeline reset, so we can use it to reset dynamic characters
        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);

            if (allowDynamicTargets)
            {
                ResetTargetsList();
            }
        }

        protected override bool UsesPCG()
        {
            return true;
        }

        private IEnumerator IE_MoveObjectDelayed(WaitForSeconds wait)
        {
            yield return wait;
            MoveObjects();
        }

        private void MoveObjects()
        {
            List<Transform> destinations = new List<Transform>(destinationsList);

            List<StatusEffectInfo> shuffledEffects = null;
            if (assignStatusEffectsToRandomizedTargets && effectsToAssign != null && effectsToAssign.Count == destinations.Count)
            {
                shuffledEffects = new List<StatusEffectInfo>(effectsToAssign);
            }

            if (randomizeDestinations && multipleDestinations)
            {
                var stream = timeline.random.Stream($"{GetUniqueName()}_DestinationsShuffle");

                int count = destinations.Count;
                for (int i = 0; i < count; i++)
                {
                    int randomIndex = stream.NextInt(i, count); // max exclusive, same as Unity int Range

                    // swap destinations
                    (destinations[i], destinations[randomIndex]) = (destinations[randomIndex], destinations[i]);

                    // swap effects in the same way, to keep pairing aligned
                    if (shuffledEffects != null)
                    {
                        // Safety guard if lists can differ
                        if (i < shuffledEffects.Count && randomIndex < shuffledEffects.Count)
                            (shuffledEffects[i], shuffledEffects[randomIndex]) = (shuffledEffects[randomIndex], shuffledEffects[i]);
                    }
                }
            }

            if (log)
            {
                if (destinations != null && destinations.Count > 0 && shuffledEffects != null && shuffledEffects.Count > 0 && destinations.Count == shuffledEffects.Count)
                {
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Assigned effects to the following destinations:");

                    for (int i = 0; i < destinations.Count; i++)
                    {
                        Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] --> Destination ({i}): {destinations[i].gameObject.name} with effect {shuffledEffects[i].name}");
                    }
                }
            }

            for (int i = 0; i < targetsList.Count; i++)
            {
                Transform target = targetsList[i];
                Transform destination = multipleDestinations ? destinations[i] : this.destination;

                if (saveEachAsRandomEventResult && multipleDestinations && FightTimeline.Instance != null)
                {
                    int resultId = baseRandomEventResultId + i;
                    FightTimeline.Instance.SetRandomEventResult(resultId, destinationsList.IndexOf(destination));
                }

                if (destination != null)
                {
                    if (multipleDestinations && assignStatusEffectsToRandomizedTargets && shuffledEffects != null)
                    {
                        StatusEffectInfo effectToAssign = shuffledEffects[i];
                        if (effectToAssign.data != null && target.TryGetComponentInChildren(true, out CharacterState c))
                        {
                            c.AddEffect(effectToAssign.data, c, false, effectToAssign.tag, effectToAssign.stacks);
                        }
                    }

                    target.position = destination.position;
                    if (rotate)
                        target.localEulerAngles = destinationRotation;
                    if (faceTarget)
                    {
                        if (multipleRotationTargets)
                        {
                            Transform currentRotationTarget = rotationTargetsList[i];
                            target.LookAt(currentRotationTarget);
                        }
                        else
                        {
                            target.LookAt(rotationTarget);
                        }
                        target.localEulerAngles = new Vector3(0, target.localEulerAngles.y, 0);
                    }
                    if (copyTargetRotation)
                    {
                        if (local)
                            target.localEulerAngles = destination.localEulerAngles;
                        else
                            target.eulerAngles = destination.eulerAngles;
                    }
                }
                else if (!relative)
                {
                    target.position = destinationPosition;
                    if (rotate)
                        target.localEulerAngles = destinationRotation;
                    if (faceTarget)
                    {
                        target.LookAt(rotationTarget);
                        target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
                    }
                    if (copyTargetRotation)
                    {
                        if (local)
                            target.localEulerAngles = destination.localEulerAngles;
                        else
                            target.eulerAngles = destination.eulerAngles;
                    }
                }
                else
                {
                    // Apply the offset only to the axes specified in destinationPosition
                    Vector3 newPosition = target.position;
                    if (local)
                        newPosition = target.localPosition;
                    newPosition.x += destinationPosition.x; // Update X if offset is non-zero
                    newPosition.y += destinationPosition.y; // Update Y if offset is non-zero
                    newPosition.z += destinationPosition.z; // Update Z if offset is non-zero
                    target.position = newPosition;

                    if (rotate)
                    {
                        Vector3 newRotation = target.eulerAngles;
                        if (local)
                            newRotation = target.localEulerAngles;
                        newRotation.x += destinationRotation.x; // Update X rotation if offset is non-zero
                        newRotation.y += destinationRotation.y; // Update Y rotation if offset is non-zero
                        newRotation.z += destinationRotation.z; // Update Z rotation if offset is non-zero
                        target.localEulerAngles = newRotation;
                    }
                    if (faceTarget)
                    {
                        if (multipleRotationTargets)
                        {
                            Transform currentRotationTarget = rotationTargetsList[i];
                            target.LookAt(currentRotationTarget);
                        }
                        else
                        {
                            target.LookAt(rotationTarget);
                        }
                        target.localEulerAngles = new Vector3(0, target.localEulerAngles.y, 0);
                    }
                }
            }
        }

        public void SetAnimators(Animator[] animators)
        {
            targetAnimators.Clear();
            targetAnimators.AddRange(animators);
        }

        public void AddActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            AddCharacter(actionInfo.source);
        }

        public void AddActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[CharacterStateMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            AddCharacter(actionInfo.target);
        }

        public void AddActionCharacters(ActionInfo actionInfo)
        {
            AddActionSourceCharacter(actionInfo);
            AddActionTargetCharacter(actionInfo);
        }

        public void AddCharacter(CharacterState character)
        {
            if (character == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to add a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping addition of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (targetsList.Contains(character.transform))
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is already in the list, skipping.");
                return;
            }

            targetsList.Add(character.transform);
        }

        public void RemoveActionSourceCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.source == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            RemoveCharacter(actionInfo.source);
        }

        public void RemoveActionTargetCharacter(ActionInfo actionInfo)
        {
            if (actionInfo.target == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }
            RemoveCharacter(actionInfo.target);
        }

        public void RemoveActionCharacters(ActionInfo actionInfo)
        {
            RemoveActionSourceCharacter(actionInfo);
            RemoveActionTargetCharacter(actionInfo);
        }

        public void RemoveCharacter(CharacterState character)
        {
            if (character == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to remove a null CharacterState, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping removal of {character.characterName} ({character.gameObject.name}).");
                return;
            }

            if (!targetsList.Contains(character.transform))
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] CharacterState {character.characterName} ({character.gameObject.name}) is not present in the list, skipping.");
                return;
            }

            targetsList.Remove(character.transform);
        }

        public void AddTarget(Transform target)
        {
            if (target == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to add a null Transform, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Dynamic targets are not allowed, skipping addition of {target.gameObject.name}.");
                return;
            }

            if (targetsList.Contains(target))
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Transform {target.gameObject.name} is already in the list, skipping.");
                return;
            }

            targetsList.Add(target);
        }

        public void RemoveTarget(Transform target)
        {
            if (target == null)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Attempted to remove a null Transform, skipping.");
                return;
            }

            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Dynamic targets are not allowed, skipping removal of {target.gameObject.name}.");
                return;
            }

            if (!targetsList.Contains(target))
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Transform {target.gameObject.name} is not present in the list, skipping.");
                return;
            }

            targetsList.Remove(target);
        }

        public void ResetTargetsList()
        {
            if (!allowDynamicTargets)
            {
                if (log)
                    Debug.Log($"[MoveObjectsMechanic ({gameObject.name})] Dynamic characters are not allowed, skipping reset of character list.");
                return;
            }

            targetsList = new List<Transform>(originalTargetsList);
        }
    }
}