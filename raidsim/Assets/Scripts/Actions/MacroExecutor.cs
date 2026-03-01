// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class MacroExecutor : MonoBehaviour
    {
        [SerializeField] private CharacterState owner;
        [SerializeField] private ChatHandler chat;
        [SerializeField] private bool executeFrameByFrame = true;

        private PartyList party;

        private void Awake()
        {
            if (owner == null)
                Debug.LogError("MacroExecutor: owner is not set");

            chat = ChatHandler.Instance;
            if (owner != null)
                party = owner.partyList;
        }

        public void Execute(MacroEntry macro)
        {
            if (!macro.isValid)
                return;

            var body = MacroParsing.NormalizeNewlines(macro.body);
            var lines = body.Split('\n');

            if (executeFrameByFrame)
            {
                StartCoroutine(IE_ExecuteMacroLines(lines));
            }
            else
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (MacroParsing.IsMacroMetaLine(line))
                        continue;

                    chat.PostUser(owner, party.GetActiveMembers(), line);
                }
            }
        }

        private IEnumerator IE_ExecuteMacroLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (MacroParsing.IsMacroMetaLine(line))
                {
                    yield return null;
                    continue;
                }

                chat.PostUser(owner, party.GetActiveMembers(), line);
                yield return null;
            }
        }
    }
}