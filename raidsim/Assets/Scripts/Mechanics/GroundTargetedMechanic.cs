using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static GlobalData;

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

    private ActionController lastActionController;
    private ActionInfo lastActionInfo;
    private GameObject groundTargetInstance;
    private bool shouldTrigger = false;

    private void Awake()
    {
        m_camera = Camera.main;
        userInput = FindObjectOfType<UserInput>();
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (actionInfo.source == null || actionInfo.action == null)
            return;
        else if (actionInfo.source != null)
            lastActionController = actionInfo.source.GetComponent<ActionController>();

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

                float range = lastActionInfo.action.data.range;
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
