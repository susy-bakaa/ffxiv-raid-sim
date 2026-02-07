// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ResetActionCooldownMechanic : FightMechanic
    {
        [Header("Reset Action Cooldown Settings")]
        public List<string> actionNames = new List<string>();
        public List<CharacterActionData> actions = new List<CharacterActionData>();
        [ShowIf(nameof(checkForStatusEffects))] public List<StatusEffectInfo> effects = new List<StatusEffectInfo>();
        public bool checkForStatusEffects = false;
        [ShowIf(nameof(checkForStatusEffects))] public bool requireAllEffects = false;

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            CharacterState state = actionInfo.target != null ? actionInfo.target : actionInfo.source;

            if (state.actionController == null)
            {
                if (log)
                    Debug.LogWarning($"[ResetActionCooldownMechanic ({gameObject.name})] Target '{state.GetCharacterName()}' does not have an ActionController. Cannot reset cooldowns.");
                return;
            }

            if (checkForStatusEffects)
            {
                bool hasRequiredEffects = false;
                foreach (StatusEffectInfo effectInfo in effects)
                {
                    if (state.HasEffect(effectInfo.data.name, effectInfo.tag))
                    {
                        hasRequiredEffects = true;
                        if (!requireAllEffects)
                            break;
                    }
                    else if (requireAllEffects)
                    {
                        hasRequiredEffects = false;
                        break;
                    }
                }
                if (!hasRequiredEffects)
                {
                    if (log)
                        Debug.Log($"[ResetActionCooldownMechanic ({gameObject.name})] Target '{state.GetCharacterName()}' does not have any of the required status effects. Cannot reset cooldowns.");
                    return;
                }
            }

            if (actionNames != null && actionNames.Count > 0)
            {
                foreach (string actionName in actionNames)
                {
                    if (state.actionController.TryGetAction(actionName, out CharacterAction action))
                    {
                        action.ResetCooldown();
                        action.ResetAnimationLock();
                        if (log)
                            Debug.Log($"[ResetActionCooldownMechanic ({gameObject.name})] Reset cooldown of '{actionName}' for '{state.GetCharacterName()}'.");
                    }
                    else
                    {
                        if (log)
                            Debug.LogWarning($"[ResetActionCooldownMechanic ({gameObject.name})] Action '{actionName}' not found for '{state.GetCharacterName()}'. Cannot reset cooldown.");
                    }
                }
            }
            else if (actions != null && actions.Count > 0)
            {
                foreach (CharacterActionData actionData in actions)
                {
                    if (state.actionController.TryGetAction(actionData, out CharacterAction action))
                    {
                        action.ResetCooldown();
                        action.ResetAnimationLock();
                        if (log)
                            Debug.Log($"[ResetActionCooldownMechanic ({gameObject.name})] Reset cooldown of '{actionData.name}' for '{state.GetCharacterName()}'.");
                    }
                    else
                    {
                        if (log)
                            Debug.LogWarning($"[ResetActionCooldownMechanic ({gameObject.name})] Action '{actionData.name}' not found for '{state.GetCharacterName()}'. Cannot reset cooldown.");
                    }
                }
            }
        }
    }
}