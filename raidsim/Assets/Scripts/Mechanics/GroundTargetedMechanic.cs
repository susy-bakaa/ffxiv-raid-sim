// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class GroundTargetedMechanic : FightMechanic
    {
        Camera m_camera;
        UserInput userInput;

        [Header("Ground Targeting Settings")]
        public CharacterState groundTargetPivot;
        public GameObject groundTargetPrefab;
        public float groundTargetPrefabElevation = 0.5f;
        public bool groundTargetActive = false;
        public UnityEvent<ActionInfo> onExecute;

        private CharacterState lastSource;
        private ActionController lastActionController;
        private ActionInfo lastActionInfo;
        private GameObject groundTargetInstance;
        private bool shouldTrigger = false;
        private CharacterState originalGroundTargetPivot;

        private void Awake()
        {
            m_camera = Camera.main;
            userInput = FindObjectOfType<UserInput>();
            originalGroundTargetPivot = groundTargetPivot;
            lastSource = null;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (actionInfo.source == null || actionInfo.action == null)
                return;
            else if (actionInfo.source != null && lastSource != actionInfo.source)
            {
                if (groundTargetPivot == null || (groundTargetPivot != null && groundTargetPivot != actionInfo.source.pivotController.GroundTargetingPivot))
                {
                    groundTargetPivot = actionInfo.source.pivotController.GroundTargetingPivot.GetComponent<CharacterState>();
                }
                lastActionController = actionInfo.source.GetComponent<ActionController>();
                lastSource = actionInfo.source;
            }

            if (groundTargetActive)
                shouldTrigger = true;

            groundTargetActive = true;
            lastActionController.isGroundTargeting = true;
            lastActionInfo = actionInfo;
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            if (lastActionController != null)
                lastActionController.isGroundTargeting = false;
            shouldTrigger = false;
            groundTargetActive = false;
            if (groundTargetInstance != null)
            {
                Destroy(groundTargetInstance);
            }
            groundTargetPivot = originalGroundTargetPivot;
            lastSource = null;
        }

        private void Update()
        {
            if (groundTargetActive)
            {
                // Get the mouse position in screen space
                Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

                // Define the plane at the specified y elevation
                Plane groundPlane = new Plane(Vector3.up, Vector3.up * groundTargetPrefabElevation);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    // Get the world position of the mouse cursor constrained to the plane
                    Vector3 mousePos = ray.GetPoint(distance);

                    float range = lastActionInfo.action.Data.range;
                    Vector3 sourcePos = lastActionInfo.source.transform.position;

                    // Clamp position within range
                    Vector3 clampedPos = sourcePos + Vector3.ClampMagnitude(mousePos - sourcePos, range);

                    // Instantiate or update the ground target instance
                    if (groundTargetInstance == null)
                    {
                        groundTargetInstance = Instantiate(groundTargetPrefab, clampedPos, Quaternion.identity);
                    }
                    else
                    {
                        groundTargetInstance.transform.position = clampedPos;
                    }

                    // Update groundTargetPivot position
                    groundTargetPivot.transform.position = clampedPos;

                    // Handle execution or cancellation
                    if (Input.GetMouseButtonDown(0) || shouldTrigger)
                    {
                        lastActionInfo.target = groundTargetPivot;
                        onExecute.Invoke(lastActionInfo);
                        groundTargetActive = false;
                        shouldTrigger = false;
                        if (lastActionController != null)
                            lastActionController.isGroundTargeting = false;
                        Destroy(groundTargetInstance);
                    }
                    else if (userInput.GetButtonDown("CancelKey"))
                    {
                        Utilities.FunctionTimer.Create(this, () => InterruptMechanic(), 0.1f, "ground_targeting_bool_delay", true, true);
                    }
                }
            }
        }
    }
}