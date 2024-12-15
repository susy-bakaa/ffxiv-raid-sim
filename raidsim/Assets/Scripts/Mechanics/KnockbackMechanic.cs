using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static ActionController;

public class KnockbackMechanic : FightMechanic
{
    public bool canBeResisted;
    public Transform origin; // Reference to the knockback source
    public Vector3 direction;
    public float strength = 1f;
    public float duration = 0.5f;
    public bool Xaxis = true;
    public bool Yaxis = false;
    public bool Zaxis = true;

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        if (!CanTrigger(actionInfo))
            return;

        if (actionInfo.source != null)
        {
            if ((!actionInfo.source.knockbackResistant.value && canBeResisted) || !canBeResisted || !actionInfo.source.bound.value)
            {
                // Calculate knockback direction
                Vector3 knockbackDirection = Vector3.zero;

                if (origin != null)
                {
                    knockbackDirection = (actionInfo.source.transform.position - origin.position).normalized;
                }
                else
                {
                    knockbackDirection = direction.normalized; // Fallback to global force direction
                }

                // Apply knockback force in the calculated direction
                Vector3 knockbackForce = knockbackDirection * direction.magnitude * strength; // * strength;

                if (!Xaxis)
                    knockbackForce = new Vector3(0f, knockbackForce.y, knockbackForce.z);
                if (!Yaxis)
                    knockbackForce = new Vector3(knockbackForce.x, 0f, knockbackForce.z);
                if (!Zaxis)
                    knockbackForce = new Vector3(knockbackForce.x, knockbackForce.y, 0f);

                // This implementation is temporary and very goofy ahh, needs a complete rework at some point
                if (actionInfo.source.playerController != null)
                {
                    actionInfo.source.playerController.Knockback(knockbackForce, duration);
                } 
                else if (actionInfo.source.aiController != null)
                {
                    actionInfo.source.aiController.Knockback(knockbackForce, duration);
                }
                else if (actionInfo.source.bossController != null)
                {
                    // Implement
                }
            }
        }
    }
}
