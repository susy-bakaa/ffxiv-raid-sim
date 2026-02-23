using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class ComboOverrideDriver : MonoBehaviour
    {
        [System.Serializable]
        public struct ComboLink
        {
            public string baseActionId;     // the slot’s base action id that should morph
            public string nextActionId;     // what it becomes
            public float windowSeconds;     // combo window duration
        }

        [SerializeField] private ActionOverrideResolver resolver;

        // You decide how to populate this: inspector, SO, generated, etc.
        [SerializeField] private ComboLink[] links;

        private readonly System.Collections.Generic.Dictionary<string, ComboLink> byBase = new();

        private void Awake()
        {
            foreach (var l in links)
                if (!string.IsNullOrWhiteSpace(l.baseActionId))
                    byBase[l.baseActionId] = l;
        }

        /// Call this from your action system when an action executes successfully.
        public void OnActionExecuted(string executedActionId)
        {
            // Example rule: executing baseActionId activates its nextActionId for windowSeconds.
            // If your combo rules are different, adapt this mapping.

            if (string.IsNullOrWhiteSpace(executedActionId))
                return;

            if (byBase.TryGetValue(executedActionId, out var link))
            {
                resolver.SetOverride("combo", link.baseActionId, link.nextActionId, link.nextActionId, link.windowSeconds, 10);
                return;
            }

            // If you want combos to break when using anything else, you can clear overrides here.
            // resolver.ClearAll();
        }

        /// Optional: call this when combo is broken (miss, wrong skill, timeout, etc.)
        public void BreakCombo(string baseActionId)
        {
            resolver.ClearOverride("combo", baseActionId);
        }
    }
}