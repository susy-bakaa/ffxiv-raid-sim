// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public sealed class MacroExecutor : MonoBehaviour
    {
        [SerializeField] private CharacterState owner;
        [SerializeField] private ChatHandler chat;

        private void Awake()
        {
            if (owner == null)
                Debug.LogError("MacroExecutor: owner is not set");

            chat = ChatHandler.Instance;
        }

        public void Execute(MacroEntry macro)
        {
            if (!macro.isValid)
                return;

            var body = MacroParsing.NormalizeNewlines(macro.body);
            var lines = body.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (MacroParsing.IsMacroMetaLine(line))
                    continue;

                // You can add more macro-only commands later (wait, echo, etc.)
                chat.PostUser(owner, line);
            }
        }
    }
}