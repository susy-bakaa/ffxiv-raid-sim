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
        [Header("References")]
        [SerializeField] private CharacterState owner;
        [SerializeField] private ChatHandler chat;

        [Header("Execution Options")]
        [SerializeField] private bool executeFrameByFrame = true;
        [SerializeField] private bool allowWaitCommands = true;

        [Header("Wait Command Options")]
        [SerializeField] private bool waitIntegerOnly = true;
        [SerializeField] private MacroParsing.WaitRoundingMode waitRoundingMode = MacroParsing.WaitRoundingMode.Ceil;
        [SerializeField] private bool waitUseUnscaledTime = false;

        private PartyList party;

        private void Start()
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

            if (allowWaitCommands)
            {
                StartCoroutine(IE_ExecuteMacroLines(lines));
            }
            else
            {
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
        }

        private IEnumerator IE_ExecuteMacroLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (MacroParsing.IsMacroMetaLine(line))
                {
                    if (executeFrameByFrame)
                        yield return null;
                    continue;
                }

                // Standalone /wait line
                if (allowWaitCommands && MacroParsing.TryParseWaitLine(line, out var waitLineSeconds, waitIntegerOnly, waitRoundingMode))
                {
                    if (waitLineSeconds > 0f)
                        yield return waitUseUnscaledTime ? new WaitForSecondsRealtime(waitLineSeconds) : new WaitForSeconds(waitLineSeconds);
                    else if (executeFrameByFrame)
                        yield return null;

                    continue;
                }

                // Inline <wait.X> (first wins)
                float inlineWaitSeconds = 0f;
                if (allowWaitCommands)
                    MacroParsing.TryExtractInlineWait(ref line, out inlineWaitSeconds, waitIntegerOnly, waitRoundingMode);

                line = line.Trim();
                if (line.Length > 0)
                    chat.PostUser(owner, party.GetActiveMembers(), line);

                // Wait after executing the line if inline wait was present; otherwise one line per frame
                if (inlineWaitSeconds > 0f)
                {
                    yield return waitUseUnscaledTime ? new WaitForSecondsRealtime(inlineWaitSeconds) : new WaitForSeconds(inlineWaitSeconds);
                }
                else if (executeFrameByFrame)
                {
                    yield return null;
                }
            }
        }
    }
}