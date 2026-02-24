// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class ComboOverrideDriver : MonoBehaviour
    {
        [System.Serializable]
        public struct ComboLink
        {
            public string baseActionId; // the slot's base action id that should morph
            public string nextActionId; // what it becomes
            public float windowSeconds; // combo window duration
        }

        [SerializeField] private ActionOverrideResolver resolver;

        [SerializeField] private ComboLink[] links;

        private readonly System.Collections.Generic.Dictionary<string, ComboLink> byBase = new();

        private void Awake()
        {
            foreach (var l in links)
                if (!string.IsNullOrWhiteSpace(l.baseActionId))
                    byBase[l.baseActionId] = l;
        }

        public void OnActionExecuted(string executedActionId)
        {

            if (string.IsNullOrWhiteSpace(executedActionId))
                return;

            if (byBase.TryGetValue(executedActionId, out var link))
            {
                resolver.SetOverride("combo", link.baseActionId, link.nextActionId, link.nextActionId, link.windowSeconds, 10);
                return;
            }

            // If combos should break when using anything else, clear overrides here.
            // resolver.ClearAll();
        }

        public void BreakCombo(string baseActionId)
        {
            resolver.ClearOverride("combo", baseActionId);
        }
    }
}