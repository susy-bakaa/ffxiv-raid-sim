// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData.Damage;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class KnockbackMechanic : FightMechanic
    {
        [Header("Knockback Settings")]
        public string knockbackName = "Knockback";
        public bool useServerSnapshot = false;
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
        private Vector3 originOffset = Vector3.zero;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (actionInfo.source != null)
            {
                CharacterSnapshot snapshot = new CharacterSnapshot();
                bool snapped = false;
                bool resisted = (actionInfo.source.knockbackResistant.value && canBeResisted) || actionInfo.source.bound.value;

                if (useServerSnapshot && FightTimeline.Instance.useServerTickSimulation)
                {
                    CharacterSnapshot? snap = FightTimeline.Instance.GetSnapshot(actionInfo.source);

                    if (snap.HasValue)
                    {
                        snapped = true;
                        snapshot = snap.Value;
                    }
                }

                if (snapped)
                    resisted = (snapshot.knockbackResistant && canBeResisted) || snapshot.bound;

                if (log)
                    Debug.Log($"[KnockbackMechanic ({gameObject.name})] use snapshot: {useServerSnapshot}, has snapshot: {snapped}");

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

                            // If using server snapshot, calculate the offset from the origin to the source's position at the time of the snapshot
                            // We can use this later to effectively trigger the knockback from the source's position at the time of the snapshot, instead of the current position
                            if (snapped)
                            {
                                originOffset = snapshot.position - actionInfo.source.transform.position;
                            }
                        }
                    }

                    // Calculate knockback direction
                    Vector3 knockbackDirection = Vector3.zero;
                    float distanceMultiplier = 1f;

                    if (origin != null)
                    {
                        knockbackDirection = ((!snapped ? actionInfo.source.transform.position : snapshot.position) - (origin.position + originOffset)).normalized;

                        if (scaleWithDistance)
                        {
                            float distance = Vector3.Distance(!snapped ? actionInfo.source.transform.position : snapshot.position, origin.position + originOffset);
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

                    float verticalDirection = direction.y;

                    if (!Xaxis)
                        knockbackForce = new Vector3(0f, knockbackForce.y, knockbackForce.z);
                    if (!Yaxis)
                        verticalDirection = 0f; // Vertical movement is handled separately in the Knockback function, so we just set this to 0 if Y-axis is disabled
                    if (!Zaxis)
                        knockbackForce = new Vector3(knockbackForce.x, knockbackForce.y, 0f);

                    // Always prevent vertical knockback force because the current implementation does not handle physics based movement upwards or down
                    // And all vertical movement is handled in the Knockback function as a simple transform movement
                    knockbackForce = new Vector3(knockbackForce.x, 0f, knockbackForce.z);

                    // This implementation is temporary and very goofy ahh, needs a complete rework at some point
                    if (actionInfo.source.playerController != null)
                    {
                        actionInfo.source.playerController.Knockback(knockbackForce, duration, verticalDirection, !disableGravity);
                    }
                    else if (actionInfo.source.aiController != null)
                    {
                        actionInfo.source.aiController.Knockback(knockbackForce, duration, verticalDirection, !disableGravity);
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

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            base.InterruptMechanic(actionInfo);
            originOffset = Vector3.zero; // Reset offset when interrupting the mechanic
        }
    }
}