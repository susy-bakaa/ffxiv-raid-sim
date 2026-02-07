// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;
using System.Collections.Generic;

namespace dev.susybaka.raidsim.UI
{
    public sealed class ChatWindow : MonoBehaviour
    {
        public static ChatWindow Instance { get; private set; }
        private UserInput input;
        private HudWindow chatBoxWindow;
        public bool isOpen => chatBoxWindow == null || chatBoxWindow.isOpen;
        public bool IsFocused => inputField != null && inputField.isFocused;

        [Header("UI")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text transcript;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Behavior")]
        [SerializeField] private bool allowRichTextFromUsers = false;
        [SerializeField] private bool autoScrollIfNearBottom = true;
        [SerializeField, Range(0f, 1f)] private float autoScrollThreshold = 0.02f;

        private CharacterState player;
        private readonly StringBuilder _sb = new(16_384);
        private bool _wantsAutoScroll = true;
        private Button temp;
        private float chatLock = 0.1f;
        private float chatLockTimer = 0f;
        private bool subbed = false;
        private List<string> sentMessages = new List<string>();
        private string currentMessageDraft = string.Empty;
        private int messageHistoryIndex = -1;

        private void Awake()
        {
            Instance = this;
            player = FightTimeline.Instance.player;
            input = FightTimeline.Instance.input;
            if (temp == null)
                temp = Utilities.FindAnyByName("Temp_RectTransform").GetComponent<Button>();
            if (chatBoxWindow == null)
                chatBoxWindow = GetComponent<HudWindow>();
            sentMessages = new List<string>();
        }

        public void OnEnable()
        {
            player = FightTimeline.Instance.player;
            input = FightTimeline.Instance.input;
            if (temp == null)
                temp = Utilities.FindAnyByName("Temp_RectTransform").GetComponent<Button>();
            if (chatBoxWindow == null)
                chatBoxWindow = GetComponent<HudWindow>();

            if (!subbed)
            {
                ChatHandler.Instance.MessageAdded += OnMessageAdded;
                ChatHandler.Instance.ChatCleared += RebuildTranscriptFromHistory;
                subbed = true;
            }

            if (scrollRect != null)
                scrollRect.onValueChanged.AddListener(_ => OnScrollChanged());

            currentMessageDraft = string.Empty;
            messageHistoryIndex = sentMessages.Count; // Reset to end of history
            RebuildTranscriptFromHistory();
        }

        public void OnDisable()
        {
            if (subbed)
            {
                if (ChatHandler.Instance != null)
                {
                    ChatHandler.Instance.MessageAdded -= OnMessageAdded;
                    ChatHandler.Instance.ChatCleared -= RebuildTranscriptFromHistory;
                }
                subbed = false;
            }

            if (scrollRect != null)
                scrollRect.onValueChanged.RemoveAllListeners();
        }

        private void Start()
        {
            if (inputField != null)
                inputField.onSubmit.AddListener(OnSubmit);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (input == null)
                return;

            if (chatLockTimer > 0f)
            {
                chatLockTimer -= Time.unscaledDeltaTime;
            }

            if (input.GetButtonDown("ActivateChatKey") && chatLockTimer <= 0f)
            {
                // If the chat box is closed, open it. Otherwise just focus the input field for quick re-activation.
                if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                    chatBoxWindow.OpenWindow();

                inputField.Select();
                inputField.ActivateInputField();
            }

            if (inputField.isFocused)
            {
                if (input.GetButtonDown("PreviousMessageKey"))
                {
                    if (sentMessages != null && sentMessages.Count > 0)
                    {
                        // Save draft only when starting to navigate history
                        if (messageHistoryIndex >= sentMessages.Count)
                        {
                            currentMessageDraft = inputField.text;
                        }
                        
                        // Move back in history
                        messageHistoryIndex--;
                        if (messageHistoryIndex < 0)
                            messageHistoryIndex = 0;

                        inputField.SetTextWithoutNotify(sentMessages[messageHistoryIndex]);
                        inputField.MoveTextEnd(false);
                    }
                }
                else if (input.GetButtonDown("NextMessageKey"))
                {
                    if (sentMessages != null && sentMessages.Count > 0)
                    {
                        // Save draft only when starting to navigate history
                        if (messageHistoryIndex >= sentMessages.Count)
                        {
                            currentMessageDraft = inputField.text;
                        }

                        // Move forward in history
                        messageHistoryIndex++;

                        if (messageHistoryIndex >= sentMessages.Count)
                        {
                            messageHistoryIndex = sentMessages.Count;
                            inputField.SetTextWithoutNotify(currentMessageDraft);
                        }
                        else
                        {
                            inputField.SetTextWithoutNotify(sentMessages[messageHistoryIndex]);
                        }
                        inputField.MoveTextEnd(false);
                    }
                }
            }

            if (input.GetButtonDown("ToggleChatKey"))
            {
                if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                    chatBoxWindow.OpenWindow();
                else if (chatBoxWindow != null && chatBoxWindow.isOpen == true)
                    chatBoxWindow.CloseWindow();

                if (inputField.isFocused)
                {
                    inputField.DeactivateInputField();
                    // Move selection to a dummy button to actually trigger deselection on the input field.
                    // Maybe there would be a better way to do this but I couldn't figure it out.
                    temp.Select();
                }
            }

            if (input.GetButtonDown("Cancel") && inputField.isFocused)
            {
                Utilities.FunctionTimer.Create(this, () => {
                    currentMessageDraft = string.Empty;
                    inputField.DeactivateInputField();
                    // Move selection to a dummy button to actually trigger deselection on the input field.
                    // Maybe there would be a better way to do this but I couldn't figure it out.
                    temp.Select();
                }, 0.5f, "ChatWindow_Cancel_DeselectDelay", true, true);
            }
        }

        private void OnSubmit(string value)
        {
            if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                return;

            if (string.IsNullOrWhiteSpace(value))
                return;

            ChatHandler.Instance.PostUser(player, value);
            sentMessages.Add(value);
            currentMessageDraft = string.Empty;
            messageHistoryIndex = sentMessages.Count; // Reset history index to end after sending

            // Lock chat input for a short time to avoid immediate re-activation.
            chatLockTimer = chatLock;
            inputField.SetTextWithoutNotify(string.Empty);
            inputField.DeactivateInputField(true);
            // Move selection to a dummy button to actually trigger deselection on the input field.
            // Maybe there would be a better way to do this but I couldn't figure it out.
            temp.Select();
        }

        private void OnScrollChanged()
        {
            if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                return;

            if (scrollRect == null)
                return;

            // verticalNormalizedPosition: 1 = top, 0 = bottom
            var nearBottom = scrollRect.verticalNormalizedPosition <= autoScrollThreshold;
            _wantsAutoScroll = !autoScrollIfNearBottom || nearBottom;
        }

        private void RebuildTranscriptFromHistory()
        {
            _sb.Clear();
            foreach (var msg in ChatHandler.Instance.History)
                AppendFormatted(msg);

            transcript.text = _sb.ToString();
            ForceScrollToBottom();
        }

        private void OnMessageAdded(ChatMessage msg)
        {
            if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                return;

            // If history capped and you want to avoid edge cases, you can occasionally rebuild.
            // For simplicity: just append.
            AppendFormatted(msg);
            transcript.text = _sb.ToString();

            if (_wantsAutoScroll)
                ForceScrollToBottom();
        }

        private void AppendFormatted(ChatMessage msg)
        {
            if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                return;

            var time = msg.TimeUtc.ToLocalTime().ToString("HH:mm");

            string text = msg.Text ?? "";
            if (!allowRichTextFromUsers && msg.Kind == ChatKind.User)
                text = EscapeRichText(text);

            // Basic styling by kind/channel (tweak to your taste)
            string color =
                msg.Kind == ChatKind.System ? "#A6A6A6" :
                msg.Kind == ChatKind.Log ? "#A6A6A6" :
                msg.Kind == ChatKind.Error ? "#FF494B" :
                msg.Kind == ChatKind.User && msg.Channel == ChatChannel.Party ? "#01FFFF" :
                msg.Kind == ChatKind.User && msg.Channel == ChatChannel.Echo ? "#A6A6A6" :
                                             "#FFFFFF";

            string senderPart = msg.Kind == ChatKind.User && msg.Channel != ChatChannel.Echo ? $"{msg.Sender}: " : "";

            _sb.Append("<color=#FFFFFF>[").Append(time).Append("]</color> ");
            _sb.Append("<color=").Append(color).Append(">");
            _sb.Append(senderPart).Append(text);
            _sb.Append("</color>\n");
        }

        private static string EscapeRichText(string s)
            => s.Replace("<", "&lt;").Replace(">", "&gt;");

        private void ForceScrollToBottom()
        {
            if (chatBoxWindow != null && chatBoxWindow.isOpen == false)
                return;

            if (scrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }

        // Convenience API for the rest of your game
        public void PostSystem(string text) => ChatHandler.Instance.PostSystem(text);
        public void PostLog(string text) => ChatHandler.Instance.PostLog(text);
        public void Clear() => ChatHandler.Instance.Clear();
    }
}