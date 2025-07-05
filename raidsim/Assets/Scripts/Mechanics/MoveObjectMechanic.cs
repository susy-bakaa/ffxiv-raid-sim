using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class MoveObjectMechanic : FightMechanic
    {
        private Animator targetAnimator;

        [Header("Move Object Settings")]
        public Transform target;
        public Transform destination;
        public bool relative = false;
        public bool local = false;
        public Vector3 destinationPosition;
        public bool rotate = false;
        [EnableIf("rotate")] public Vector3 destinationRotation;
        public bool faceTarget = false;
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
            if (target != null)
                targetAnimator = target.GetComponentInChildren<Animator>();
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (targetAnimator != null && !string.IsNullOrEmpty(triggerAnimation))
            {
                if (playDirectly)
                    targetAnimator.CrossFadeInFixedTime(triggerAnimationHash, 0.2f);
                else
                    targetAnimator.SetTrigger(triggerAnimationHash);
            }

            if (animationDuration > 0)
            {
                if (ieMoveObjectDelayed == null)
                    ieMoveObjectDelayed = StartCoroutine(IE_MoveObjectDelayed(new WaitForSeconds(animationDuration)));
            }
            else
            {
                MoveObject();
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);

            if (ieMoveObjectDelayed != null)
            {
                StopCoroutine(ieMoveObjectDelayed);
                ieMoveObjectDelayed = null;
            }
        }

        private IEnumerator IE_MoveObjectDelayed(WaitForSeconds wait)
        {
            yield return wait;
            MoveObject();
            ieMoveObjectDelayed = null;
        }

        private void MoveObject()
        {
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

        public void SetAnimator(Animator animator)
        {
            targetAnimator = animator;
        }
    }
}