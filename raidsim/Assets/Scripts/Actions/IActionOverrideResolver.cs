// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).

namespace dev.susybaka.raidsim.Actions 
{
    public enum ActionResolveMode
    {
        Presentation, // icon/text/tooltip/cooldown UI
        Execution     // what actually executes when pressed
    }

    public interface IActionOverrideResolver
    {
        /// Returns an effective ActionId to use instead of baseActionId.
        /// Return null/empty to indicate "no override".
        string ResolveActionId(string baseActionId, ActionResolveMode mode);
    }
}