// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class MoveObjectsMechanic : FightMechanic
    {
        private Animator[] targetAnimators;

        [Header("Move Object Settings")]
        public Transform[] targets;
        [HideIf("multipleDestinations")] public Transform destination;
        [ShowIf("multipleDestinations")] public Transform[] destinations;
        public bool multipleDestinations = false;
        public bool relative = false;
        public bool local = false;
        public Vector3 destinationPosition;
        public bool rotate = false;
        [EnableIf("rotate")] public Vector3 destinationRotation;
        public bool faceTarget = false;
        public bool copyTargetRotation = false;
        [EnableIf("faceTarget")]
        public Transform rotationTarget;
        public float animationDuration = -1f;
        public string triggerAnimation = string.Empty;
        public bool playDirectly = false;

        private int triggerAnimationHash;
        Coroutine ieMoveObjectDelayed;

        private void Start()
        {
            triggerAnimationHash = Animator.StringToHash(triggerAnimation);

            if (multipleDestinations)
            {
                destination = null;

                if (destinations == null || targets == null)
                {
                    Debug.LogError("Multiple destinations selected but targets or destinations are missing!");
                    return;
                }
                if (destinations.Length != targets.Length)
                {
                    Debug.LogError("Multiple destinations selected but the number of targets and destinations do not match!");
                    return;
                }
            }

            if (targets != null)
            {
                targetAnimators = new Animator[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    targetAnimators[i] = targets[i].GetComponentInChildren<Animator>();
                }
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (targetAnimators != null && !string.IsNullOrEmpty(triggerAnimation))
            {
                for (int i = 0; i < targetAnimators.Length; i++)
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

        private IEnumerator IE_MoveObjectDelayed(WaitForSeconds wait)
        {
            yield return wait;
            MoveObjects();
        }

        private void MoveObjects()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                Transform destination = multipleDestinations ? destinations[i] : this.destination;

                if (destination != null)
                {
                    target.position = destination.position;
                    if (rotate)
                        target.localEulerAngles = destinationRotation;
                    if (faceTarget)
                    {
                        target.LookAt(rotationTarget);
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
                        target.LookAt(rotationTarget);
                        target.localEulerAngles = new Vector3(0, target.localEulerAngles.y, 0);
                    }
                }
            }
        }

        public void SetAnimators(Animator[] animators)
        {
            targetAnimators = animators;
        }
    }
}