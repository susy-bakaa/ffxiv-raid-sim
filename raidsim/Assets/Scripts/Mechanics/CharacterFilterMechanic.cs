using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CharacterFilterMechanic : FightMechanic
    {
        public bool filterGhosts = false;
        [ShowIf(nameof(filterGhosts))] public bool allow = false;
        [HideIf(nameof(filterGhosts))] public List<string> allowCharacters = new List<string>();
        public FightMechanic mechanic;

        private void OnValidate()
        {
            if (filterGhosts)
            {
                allowCharacters.Clear();
            }
            else
            {
                allow = false;
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (allowCharacters != null && allowCharacters.Count > 0)
            {
                if (actionInfo.source != null || actionInfo.target != null)
                {
                    bool allowTrigger = false;

                    for (int i = 0; i < allowCharacters.Count; i++)
                    {
                        bool? sourceCheck = actionInfo.source?.GetCharacterName().Contains(allowCharacters[i]);
                        bool? targetCheck = actionInfo.target?.GetCharacterName().Contains(allowCharacters[i]);

                        if ((sourceCheck != null && sourceCheck.Value) || (targetCheck != null && targetCheck.Value))
                        {
                            allowTrigger = true;
                            break;
                        }
                    }

                    if (!allowTrigger)
                    {
                        if (log)
                            Debug.Log($"[CharacterFilterMechanic ({gameObject.name})] Prevented '{(actionInfo.source != null ? actionInfo.source.GetCharacterName() : actionInfo.target != null ? actionInfo.target.GetCharacterName() : "Unknown Character")}' from triggering mechanic.");
                        return;
                    }
                }
            }
            else if (filterGhosts)
            {
                if (allow)
                {
                    if ((actionInfo.source != null && actionInfo.source.ghost) || (actionInfo.target != null && actionInfo.target.ghost))
                    {
                        if (log)
                            Debug.Log($"[CharacterFilterMechanic ({gameObject.name})] Prevented '{(actionInfo.source != null ? actionInfo.source.GetCharacterName() : actionInfo.target != null ? actionInfo.target.GetCharacterName() : "Unknown Character")}' from triggering mechanic because they are a ghost.");
                        return;
                    }
                }
                else
                {
                    if ((actionInfo.source != null && !actionInfo.source.ghost) || (actionInfo.target != null && !actionInfo.target.ghost))
                    {
                        if (log)
                            Debug.Log($"[CharacterFilterMechanic ({gameObject.name})] Prevented '{(actionInfo.source != null ? actionInfo.source.GetCharacterName() : actionInfo.target != null ? actionInfo.target.GetCharacterName() : "Unknown Character")}' from triggering mechanic because they are NOT a ghost.");
                        return;
                    }
                }
            }

            if (log)
                Debug.Log($"[CharacterFilterMechanic ({gameObject.name})] Triggered mechanic for '{(actionInfo.source != null ? actionInfo.source.GetCharacterName() : actionInfo.target != null ? actionInfo.target.GetCharacterName() : "Unknown Character")}'.");

            if (mechanic != null)
                mechanic.TriggerMechanic(actionInfo);
        }
    }
}