using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalData;
using static GlobalData.Damage;
using static UnityEngine.GraphicsBuffer;

public class KnockbackMechanic : FightMechanic
{
    [Header("Knockback Settings")]
    public string knockbackName = "Knockback";
    public bool showDamagePopup = false;
    public bool canBeResisted;
    public bool originFromSource = false;
    public bool isDash = false;
    public CharacterState source;
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
            bool resisted = (actionInfo.source.knockbackResistant.value && canBeResisted) || actionInfo.source.bound.value;

            if (!resisted)
            {
                if (originFromSource)
                {
                    if (isDash && actionInfo.source.dashKnockbackPivot != null)
                    {
                        origin = actionInfo.source.dashKnockbackPivot;
                    }
                    else
                    {
                        origin = actionInfo.source.transform;
                    }
                }

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
            else
            {
                if (!actionInfo.source.dead && showDamagePopup)
                    actionInfo.source.ShowDamageFlyText(new Damage(0, true, true, DamageType.none, ElementalAspect.unaspected, PhysicalAspect.none, DamageApplicationType.normal, source, knockbackName));
                if (log)
                    Debug.Log($"[KnockbackMechanic ({gameObject.name})] {actionInfo.source.characterName} ({actionInfo.source.gameObject.name}) resisted the knockback!");
            }
        }
    }
}
