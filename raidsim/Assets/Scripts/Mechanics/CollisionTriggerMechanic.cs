// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CollisionTriggerMechanic : MonoBehaviour
    {
        [Label("ID")]
        public int id = 0;
        [Tag]
        public string tagToCollideWith = "Player";
        public List<StatusEffectInfo> requiredEffect;
        public bool requireOnlyOneMatchingEffect = true;
        public UnityEvent<int> onCollisionEnter;
        public UnityEvent<int> onCollisionExit;
        public bool log = false;

        private void OnTriggerEnter(Collider other)
        {
            if (log)
            {
                if (!other.gameObject.name.ToLower().Contains("aoe_") && !other.gameObject.name.ToLower().Contains("tower_"))
                    Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] Detected a collision with {other.gameObject.name} in enter");
            }

            if (other.CompareTag(tagToCollideWith))
            {
                if (log)
                    Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] collider {other.gameObject.name} has the right tag (enter)");

                if (other.transform.TryGetComponentInParents(true, out CharacterState state))
                {
                    if (log)
                        Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] collider {other.gameObject.name} has the CharacterState component (enter)");
                    if (requiredEffect != null && requiredEffect.Count > 0)
                    {
                        if (requireOnlyOneMatchingEffect)
                        {
                            foreach (StatusEffectInfo effect in requiredEffect)
                            {
                                if (state.HasAnyVersionOfEffect(effect.data.statusName))
                                {
                                    if (log)
                                        Debug.Log("Entered collision with " + tagToCollideWith + " with required effect " + effect.data.statusName);
                                    onCollisionEnter.Invoke(id);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            bool hasAllEffects = true;
                            foreach (StatusEffectInfo effect in requiredEffect)
                            {
                                if (!state.HasAnyVersionOfEffect(effect.data.statusName))
                                {
                                    hasAllEffects = false;
                                    break;
                                }
                            }
                            if (hasAllEffects)
                            {
                                if (log)
                                    Debug.Log("Entered collision with " + tagToCollideWith + " with all required effects");
                                onCollisionEnter.Invoke(id);
                            }
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (log)
            {
                if (!other.gameObject.name.ToLower().Contains("aoe_") && !other.gameObject.name.ToLower().Contains("tower_"))
                    Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] Detected a collision with {other.gameObject.name} in exit");
            }

            if (other.CompareTag(tagToCollideWith))
            {
                if (log)
                    Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] collider {other.gameObject.name} has the right tag (exit)");
                if (other.transform.TryGetComponentInParents(true, out CharacterState state))
                {
                    if (log)
                        Debug.Log($"[CollisionTriggerMechanic ({gameObject.name})] collider {other.gameObject.name} has the CharacterState component (exit)");
                    if (requiredEffect != null && requiredEffect.Count > 0)
                    {
                        if (requireOnlyOneMatchingEffect)
                        {
                            foreach (StatusEffectInfo effect in requiredEffect)
                            {
                                if (state.HasAnyVersionOfEffect(effect.data.statusName))
                                {
                                    if (log)
                                        Debug.Log("Exited collision with " + tagToCollideWith + " with required effect " + effect.data.statusName);
                                    onCollisionExit.Invoke(id);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            bool hasAllEffects = true;
                            foreach (StatusEffectInfo effect in requiredEffect)
                            {
                                if (!state.HasAnyVersionOfEffect(effect.data.statusName))
                                {
                                    hasAllEffects = false;
                                    break;
                                }
                            }
                            if (hasAllEffects)
                            {
                                if (log)
                                    Debug.Log("Exited collision with " + tagToCollideWith + " with all required effects");
                                onCollisionExit.Invoke(id);
                            }
                        }
                    }
                }
            }
        }
    }

}