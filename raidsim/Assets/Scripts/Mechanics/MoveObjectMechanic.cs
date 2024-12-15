using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MoveObjectMechanic : FightMechanic
{
    public Transform target;
    public Transform destination;
    public bool relative = false;
    public bool local = false;
    public Vector3 destinationPosition;
    public bool rotate = false;
    [EnableIf("rotate")] public Vector3 destinationRotation;

    public override void TriggerMechanic(ActionController.ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (destination != null)
        {
            target.position = destination.position;
            if (rotate)
                target.localEulerAngles = destinationRotation;
        }
        else if (!relative)
        {
            target.position = destinationPosition;
            if (rotate)
                target.localEulerAngles = destinationRotation;
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
        }
    }
}
