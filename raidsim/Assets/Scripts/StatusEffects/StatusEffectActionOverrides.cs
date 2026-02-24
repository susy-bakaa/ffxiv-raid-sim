// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.StatusEffects
{
    public sealed class StatusEffectActionOverrides : MonoBehaviour
    {
        [Serializable]
        public struct OverrideRule
        {
            [Tooltip("Base action id stored in hotbar slots.")]
            public string baseActionId;

            [Tooltip("Action id to execute while status is active.")]
            public string executionActionId;

            [Tooltip("Optional: action id to show in UI. If empty, executionActionId is used.")]
            public string presentationActionId;

            [Tooltip("Higher wins when multiple overrides exist.")]
            public int priority;
        }

        [SerializeField] private string statusKey = ""; // optional stable key like "surecast_proc"
        [SerializeField] private OverrideRule[] overrides;

        // Use a per-instance sourceKey so stacking doesn't clobber.
        private string SourceKey => string.IsNullOrWhiteSpace(statusKey)
            ? $"status_{GetInstanceID()}"
            : $"status_{statusKey}_{GetInstanceID()}";

        public void Apply(CharacterState character)
        {
            var resolver = GetResolver(character);
            if (resolver == null)
                return;

            var src = SourceKey;

            for (int i = 0; i < overrides.Length; i++)
            {
                var r = overrides[i];
                if (string.IsNullOrWhiteSpace(r.baseActionId))
                    continue;

                var execId = r.executionActionId ?? "";
                var presId = string.IsNullOrWhiteSpace(r.presentationActionId) ? execId : r.presentationActionId;

                resolver.SetOverride(
                    sourceKey: src,
                    baseId: r.baseActionId,
                    executionId: execId,
                    presentationId: presId,
                    durationSeconds: 0f,  // status removal will clear
                    priority: r.priority
                );
            }
        }

        public void Remove(CharacterState character)
        {
            var resolver = GetResolver(character);
            if (resolver == null)
                return;

            var src = SourceKey;

            for (int i = 0; i < overrides.Length; i++)
            {
                var r = overrides[i];
                if (string.IsNullOrWhiteSpace(r.baseActionId))
                    continue;
                resolver.ClearOverride(src, r.baseActionId);
            }
        }

        private static ActionOverrideResolver GetResolver(CharacterState character)
        {
            if (character == null)
                return null;

            var resolver = character.hotbarController?.OverrideResolver;

            return resolver != null ? resolver : character.GetComponentInChildren<ActionOverrideResolver>();
        }
    }
}