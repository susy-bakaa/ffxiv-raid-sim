// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData.Damage;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class KnockbackMechanic : FightMechanic
    {
        [Header("Knockback Settings")]
        public string knockbackName = "Knockback";
        public bool showDamagePopup = false;
        public bool canBeResisted;
        public bool originFromSource = false;
        public bool isDash = false;
        public bool scaleWithDistance = false;
        [ShowIf(nameof(scaleWithDistance))] public Vector2 distanceRange = new Vector2(0.5f, 3f);
        [ShowIf(nameof(scaleWithDistance))] public Vector2 distanceMultipliers = new Vector2(0.5f, 1f);
        public bool disableGravity = false;
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
                    float distanceMultiplier = 1f;

                    if (origin != null)
                    {
                        knockbackDirection = (actionInfo.source.transform.position - origin.position).normalized;

                        if (scaleWithDistance)
                        {
                            float distance = Vector3.Distance(actionInfo.source.transform.position, origin.position);
                            distanceMultiplier = Utilities.Map(Mathf.Clamp(distance, distanceRange.x, distanceRange.y), distanceRange.x, distanceRange.y, distanceMultipliers.x, distanceMultipliers.y); // Clamp to prevent extreme values
                        }
                    }
                    else
                    {
                        knockbackDirection = direction.normalized; // Fallback to global force direction
                    }

                    // Apply knockback force in the calculated direction
                    Vector3 knockbackForce = knockbackDirection * direction.magnitude * strength;

                    if (scaleWithDistance && origin != null)
                    {
                        // Scale horizontal components with distance (stronger when farther)
                        knockbackForce.x *= distanceMultiplier;
                        knockbackForce.z *= distanceMultiplier;
                        // Scale vertical component inversely (stronger when closer for higher arc, weaker when farther for flatter trajectory)
                        float inverseMultiplier = distanceMultipliers.x + distanceMultipliers.y - distanceMultiplier;
                        knockbackForce.y *= inverseMultiplier;
                    }

                    if (!Xaxis)
                        knockbackForce = new Vector3(0f, knockbackForce.y, knockbackForce.z);
                    if (!Yaxis)
                        knockbackForce = new Vector3(knockbackForce.x, 0f, knockbackForce.z);
                    if (!Zaxis)
                        knockbackForce = new Vector3(knockbackForce.x, knockbackForce.y, 0f);

                    // This implementation is temporary and very goofy ahh, needs a complete rework at some point
                    if (actionInfo.source.playerController != null)
                    {
                        actionInfo.source.playerController.Knockback(knockbackForce, duration, direction.y, !disableGravity);
                    }
                    else if (actionInfo.source.aiController != null)
                    {
                        actionInfo.source.aiController.Knockback(knockbackForce, duration, direction.y, !disableGravity);
                    }
                    else if (actionInfo.source.bossController != null)
                    {
                        // Implement
                    }
                    if (log)
                        Debug.Log($"[KnockbackMechanic ({gameObject.name})] {actionInfo.source.characterName} ({actionInfo.source.gameObject.name}) got hit by the knockback!");
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
}