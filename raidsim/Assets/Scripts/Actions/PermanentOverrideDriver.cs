// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class PermanentOverrideDriver : MonoBehaviour
    {
        [Serializable]
        public struct Rule
        {
            [Tooltip("Base ActionId (the id stored in hotbar slots / palette)")]
            public string baseActionId;

            [Tooltip("ActionId to execute instead (character-specific variant)")]
            public string executionActionId;

            [Tooltip("Optional: ActionId to show in UI. If empty, executionActionId is used.")]
            public string presentationActionId;

            [Tooltip("Higher wins. Use e.g. 0 for baseline, 50 to beat combos, 100 to beat almost everything.")]
            public int priority;
        }

        [SerializeField] private ActionOverrideResolver resolver;
        [SerializeField] private CharacterActionRegistry registry; // optional validation
        [SerializeField] private Rule[] rules;

        private const string SourceKey = "permanent";
        private bool hasBeenDisabled = false;

        private void Reset()
        {
            if (!resolver)
                resolver = GetComponent<ActionOverrideResolver>();
            if (!registry)
                registry = GetComponentInChildren<CharacterActionRegistry>();
        }

        private void Start()
        {
            if (!resolver)
                resolver = GetComponent<ActionOverrideResolver>();
            ApplyAll();
        }

        private void OnEnable()
        {
            // We need to do this to ensure that the action registry has fully finished setup before we try to apply overrides,
            // otherwise we might get false warnings about missing ActionIds. This happens because OnEnable runs too soon, so we do initial setup inside Start instead.
            if (!hasBeenDisabled)
                return;

            if (!resolver)
                resolver = GetComponent<ActionOverrideResolver>();
            ApplyAll();
        }

        private void OnDisable()
        {
            hasBeenDisabled = true;
            if (resolver)
                resolver.ClearAllFromSource(SourceKey);
        }

        public void ApplyAll()
        {
            if (!resolver)
                return;

            for (int i = 0; i < rules.Length; i++)
            {
                var r = rules[i];
                if (string.IsNullOrWhiteSpace(r.baseActionId) || string.IsNullOrWhiteSpace(r.executionActionId))
                    continue;

                // Optional validation (won’t stop anything, just warns)
                if (registry)
                {
                    if (registry.GetById(r.baseActionId) == null)
                        Debug.LogWarning($"Permanent override baseActionId not found: '{r.baseActionId}'", this);

                    if (registry.GetById(r.executionActionId) == null)
                        Debug.LogWarning($"Permanent override executionActionId not found: '{r.executionActionId}'", this);

                    if (!string.IsNullOrWhiteSpace(r.presentationActionId) && registry.GetById(r.presentationActionId) == null)
                        Debug.LogWarning($"Permanent override presentationActionId not found: '{r.presentationActionId}'", this);
                }

                var pres = string.IsNullOrWhiteSpace(r.presentationActionId) ? r.executionActionId : r.presentationActionId;

                resolver.SetOverride(
                    sourceKey: SourceKey,
                    baseId: r.baseActionId,
                    executionId: r.executionActionId,
                    presentationId: pres,
                    durationSeconds: 0f,
                    priority: r.priority
                );
            }
        }
    }
}