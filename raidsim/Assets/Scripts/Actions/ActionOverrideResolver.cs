using System;
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class ActionOverrideResolver : MonoBehaviour, IActionOverrideResolver
    {
        [Serializable]
        private struct OverrideEntry
        {
            public string sourceKey; // e.g. "combo", "status:name", "stance"
            public string baseId;

            public string presentationId; // can be empty -> use executionId or none
            public string executionId;    // can be empty -> no override

            public float expiresAt;      // Time.time, <=0 means no expiry
            public int priority;         // higher wins
        }

        // baseId -> list of overrides from different sources
        private readonly Dictionary<string, List<OverrideEntry>> overrides = new();

        public event Action OnOverridesChanged;

        public string ResolveActionId(string baseActionId, ActionResolveMode mode)
        {
            if (string.IsNullOrWhiteSpace(baseActionId))
                return null;

            if (!overrides.TryGetValue(baseActionId, out var list) || list == null || list.Count == 0)
                return null;

            // Remove expired entries (in-place), pick best remaining by priority
            OverrideEntry best = default;
            bool found = false;
            bool changed = false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var e = list[i];
                if (e.expiresAt > 0f && Time.time >= e.expiresAt)
                {
                    list.RemoveAt(i);
                    changed = true;
                    continue;
                }

                if (!found || e.priority > best.priority)
                {
                    best = e;
                    found = true;
                }
            }

            if (list.Count == 0)
            {
                overrides.Remove(baseActionId);
                changed = true;
            }

            if (changed)
                OnOverridesChanged?.Invoke();

            if (!found)
                return null;

            // Choose id based on mode
            string id = mode == ActionResolveMode.Presentation ? best.presentationId : best.executionId;

            // If presentation override not provided, fall back to execution override
            if (mode == ActionResolveMode.Presentation && string.IsNullOrWhiteSpace(id))
                id = best.executionId;

            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        /// Add or replace an override for (sourceKey, baseId).
        /// - If you call it again with same sourceKey+baseId, it overwrites that entry only.
        /// - durationSeconds <= 0 => no expiry.
        public void SetOverride(
            string sourceKey,
            string baseId,
            string executionId,
            string presentationId = null,
            float durationSeconds = 0f,
            int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(sourceKey) || string.IsNullOrWhiteSpace(baseId))
                return;

            // Empty executionId means "remove this override from this source"
            if (string.IsNullOrWhiteSpace(executionId) && string.IsNullOrWhiteSpace(presentationId))
            {
                ClearOverride(sourceKey, baseId);
                return;
            }

            float expiresAt = durationSeconds > 0f ? Time.time + durationSeconds : 0f;

            var entry = new OverrideEntry
            {
                sourceKey = sourceKey,
                baseId = baseId,
                executionId = executionId ?? "",
                presentationId = presentationId ?? "",
                expiresAt = expiresAt,
                priority = priority
            };

            if (!overrides.TryGetValue(baseId, out var list) || list == null)
            {
                list = new List<OverrideEntry>(2);
                overrides[baseId] = list;
            }

            // Overwrite existing entry from same sourceKey
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].sourceKey == sourceKey)
                {
                    list[i] = entry;
                    OnOverridesChanged?.Invoke();
                    return;
                }
            }

            list.Add(entry);
            OnOverridesChanged?.Invoke();
        }

        public void ClearOverride(string sourceKey, string baseId)
        {
            if (string.IsNullOrWhiteSpace(sourceKey) || string.IsNullOrWhiteSpace(baseId))
                return;

            if (!overrides.TryGetValue(baseId, out var list) || list == null)
                return;

            int removed = list.RemoveAll(e => e.sourceKey == sourceKey);
            if (removed > 0)
            {
                if (list.Count == 0)
                    overrides.Remove(baseId);

                OnOverridesChanged?.Invoke();
            }
        }

        public void ClearAllFromSource(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                return;

            bool changed = false;

            // This is O(n) across all overrides, but counts are small.
            var keys = new List<string>(overrides.Keys);
            foreach (var baseId in keys)
            {
                var list = overrides[baseId];
                int removed = list.RemoveAll(e => e.sourceKey == sourceKey);
                if (removed > 0)
                {
                    changed = true;
                    if (list.Count == 0)
                        overrides.Remove(baseId);
                }
            }

            if (changed)
                OnOverridesChanged?.Invoke();
        }

        public void ClearAll()
        {
            if (overrides.Count == 0)
                return;
            overrides.Clear();
            OnOverridesChanged?.Invoke();
        }
    }
}