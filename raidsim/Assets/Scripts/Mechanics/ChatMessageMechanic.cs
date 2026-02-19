// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ChatMessageMechanic : FightMechanic
    {
        ChatWindow chatWindow;

        [Header("Chat Message Mechanic")]
        [Tooltip("If enabled, the chat will be cleared instead of posting a message.")]
        public bool clearChatInstead = false;
        [HideIf(nameof(clearChatInstead))] public List<string> messages = new List<string>() { "Hello World" };
        [ShowIf(nameof(_showIndexMapping))] public List<IndexMapping> indexMapping = new List<IndexMapping>();
        [HideIf(nameof(clearChatInstead))] public int fightTimelineRandomEventId = -1;
        [Tooltip("If enabled, a message will be picked based on a previously stored FightTimelineRandomEventId result.")]
        [HideIf(nameof(_hidePickBasedOnResult))] public bool pickBasedOnResult = false;
        [Tooltip("If enabled, a random message will be picked and the result stored a FightTimelineRandomEventId.")]
        [HideIf(nameof(_hidePickRandom))] public bool pickRandom = true;
        [Tooltip("If enabled, FightTimelineRandomEventId results will be mapped.")]
        [HideIf(nameof(clearChatInstead))] public bool useIndexMapping = false;
        [Tooltip("If enabled, messages are sent as System Messages.")]
        [HideIf(nameof(clearChatInstead))] public bool systemMessage = true;
        [Tooltip("If enabled and the final message ends up empty, it will not be sent at all.")]
        [HideIf(nameof(clearChatInstead))] public bool discardEmptyMessages = true;

        private bool _hidePickBasedOnResult => pickRandom || clearChatInstead;
        private bool _hidePickRandom => pickBasedOnResult || clearChatInstead;
        private bool _showIndexMapping => useIndexMapping && !clearChatInstead;

        private void Awake()
        {
            chatWindow = ChatWindow.Instance;
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (clearChatInstead && ChatHandler.Instance != null)
            {
                if (log)
                    Debug.Log($"[ChatMessageMechanic ({gameObject.name})] Clearing chat.");
                ChatHandler.Instance.Clear();
                return;
            }    

            if (chatWindow == null)
                chatWindow = ChatWindow.Instance;

            if (chatWindow != null)
            {
                string msg = string.Empty;

                if (pickBasedOnResult && fightTimelineRandomEventId > -1 && FightTimeline.Instance != null && FightTimeline.Instance.TryGetRandomEventResult(fightTimelineRandomEventId, out int r))
                {
                    if (useIndexMapping)
                    {
                        foreach (IndexMapping mapping in indexMapping)
                        {
                            if (mapping.previousIndex == r)
                            {
                                r = mapping.nextIndex;
                                break;
                            }
                        }
                    }

                    msg = messages[r];
                } 
                else if (fightTimelineRandomEventId > -1 && FightTimeline.Instance != null)
                {
                    if (pickRandom)
                    {
                        r = timeline.random.Pick($"{GetUniqueName()}_PickRandom", messages.Count, timeline.GlobalRngMode);
                        msg = messages[r];
                        FightTimeline.Instance.AddRandomEventResult(fightTimelineRandomEventId, r);
                    }
                }

                if (discardEmptyMessages && string.IsNullOrEmpty(msg))
                {
                    if (log)
                        Debug.Log($"[ChatMessageMechanic ({gameObject.name})] Final message was empty, skipping.");
                    return;
                }
                else if (log)
                {
                    Debug.Log($"[ChatMessageMechanic ({gameObject.name})] Sending message '{msg}'");
                }

                if (systemMessage)
                {
                    chatWindow.PostSystem(msg);
                }
                else
                {
                    chatWindow.PostLog(msg);
                }
            }
        }

        protected override bool UsesPCG()
        {
            return true;
        }
    }
}