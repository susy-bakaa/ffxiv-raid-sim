using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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