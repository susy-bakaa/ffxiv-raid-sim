// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.Globalization;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Core
{
    public sealed class ChatHandler
    {
        public static ChatHandler Instance => _instance ??= new ChatHandler();
        private static ChatHandler _instance;

        public event Action<ChatMessage> MessageAdded;
        public event Action ChatCleared;

        public IReadOnlyList<ChatMessage> History => _history;

        private readonly List<ChatMessage> _history = new(512);

        private long _nextId = 1;
        private int _maxHistory = 500;

        public int MaxHistory
        {
            get => _maxHistory;
            set => _maxHistory = Math.Max(50, value);
        }

        private ChatHandler() { }

        public void PostUser(CharacterState sender, List<CharacterState> targets, string text, ChatChannel channel = ChatChannel.Party)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            string name = sender != null ? sender.characterName : "User";

            // commands/macros start with "/"
            string trimmedText = text.Trim();
            if (TryExecuteCommand(sender, targets, trimmedText, ref channel))
                return;

            // If the text started with a command that just changed the channel,
            // remove the command prefix from the message
            if (trimmedText.StartsWith("/"))
            {
                int firstSpace = trimmedText.IndexOf(' ');
                if (firstSpace > 0)
                {
                    // Strip the command and keep the rest
                    trimmedText = trimmedText[(firstSpace + 1)..].Trim();
                }
                else
                {
                    // Just the command with no arguments, nothing to post
                    return;
                }
            }

            Add(new ChatMessage(_nextId++, DateTime.UtcNow, name, ChatKind.User, channel, trimmedText));
        }

        public void PostSystem(string text, ChatKind kind = ChatKind.System, ChatChannel channel = ChatChannel.System)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            Add(new ChatMessage(_nextId++, DateTime.UtcNow, "System", kind, channel, text.Trim()));
        }

        public void PostLog(string text, ChatKind kind = ChatKind.Log, ChatChannel channel = ChatChannel.Debug)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            Add(new ChatMessage(_nextId++, DateTime.UtcNow, "Log", kind, channel, text.Trim()));
        }

        public void Clear()
        {
            _history.Clear();
            ChatCleared?.Invoke();
            //MessageAdded?.Invoke(new ChatMessage(_nextId++, DateTime.UtcNow, "System", ChatKind.System, ChatChannel.System, "Chat cleared."));
        }

        private void Add(ChatMessage msg)
        {
            _history.Add(msg);

            if (_history.Count > _maxHistory)
                _history.RemoveRange(0, _history.Count - _maxHistory);

            MessageAdded?.Invoke(msg);
        }

        private bool TryExecuteCommand(CharacterState sender, List<CharacterState> targets, string raw, ref ChatChannel channel)
        {
            if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith("/"))
                return false;

            // Split "/cmd rest of line"
            int firstSpace = raw.IndexOf(' ');
            string cmd = (firstSpace < 0 ? raw[1..] : raw[1..firstSpace]).Trim();
            string args = (firstSpace < 0 ? "" : raw[(firstSpace + 1)..]).Trim();

            if (cmd.Length == 0)
                return true;

            switch (cmd.ToLowerInvariant())
            {
                case "clear":
                    Clear();
                    return true;

                case "cl":
                    Clear();
                    return true;

                // /echo some text -> post as user but only to echo channel (not broadcast to party)
                case "echo":
                    channel = ChatChannel.Echo;
                    return false;

                case "e":
                    channel = ChatChannel.Echo;
                    return false;

                case "party":
                    channel = ChatChannel.Party;
                    return false;

                case "p":
                    channel = ChatChannel.Party;
                    return false;

                case "enemysign":
                    return MarkCommand(sender, targets, args);

                case "marking":
                    return MarkCommand(sender, targets, args);

                case "mark":
                    return MarkCommand(sender, targets, args);

                case "mk":
                    return MarkCommand(sender, targets, args);

                // /time, /t, /lt -> convenient local timestamp insert and then /servertime, /stime, /st -> UTC timestamp
                case "time":
                    PostSystem(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                case "t":
                    PostSystem(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                case "lt":
                    PostSystem(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                case "servertime":
                    PostSystem(DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                case "stime":
                    PostSystem(DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                case "st":
                    PostSystem(DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
                    return true;

                // /action some text -> execute character actions
                case "action":
                    return ActionCommand(sender, targets, args);

                case "ac":
                    return ActionCommand(sender, targets, args);

                case "blueaction":
                    return ActionCommand(sender, targets, args);

                // targeting
                case "targetenemy":
                    sender?.targetController?.CycleTarget();
                    return true;

                case "tenemy":
                    sender?.targetController?.CycleTarget();
                    return true;

                case "nexttarget":
                    sender?.targetController?.CycleTarget();
                    return true;

                case "nt":
                    sender?.targetController?.CycleTarget();
                    return true;

                case "facetarget":
                    sender?.targetController?.FaceTarget();
                    return true;

                case "ft":
                    sender?.targetController?.FaceTarget();
                    return true;

                // /pause -> toggle timeline pause/resume
                case "pause":
                    if (FightTimeline.Instance == null)
                    {
                        PostSystem("No active timeline to pause/resume.", ChatKind.Error);
                        return true;
                    }
                    FightTimeline.Instance.TogglePause("menubar_pause");

                    if (FightTimeline.Instance.paused)
                        PostSystem("Timeline paused.");
                    else
                        PostSystem("Timeline resumed.");
                    return true;

                // /reset -> soft reset timeline to start
                case "reset":
                    if (FightTimeline.Instance == null)
                    {
                        PostSystem("No active timeline to reset.", ChatKind.Error);
                        return true;
                    }
                    FightTimeline.Instance.ResetTimeline(1f);
                    PostSystem("Timeline reset.");
                    return true;

                // /reload -> hard reset timeline to start
                case "reload":
                    if (FightTimeline.Instance == null)
                    {
                        PostSystem("No active timeline to reload.", ChatKind.Error);
                        return true;
                    }
                    FightTimeline.Instance.fightSelector.Reload();
                    PostSystem("Timeline reloaded.");
                    return true;

                // /start -> start timeline
                case "start":
                    if (FightTimeline.Instance == null)
                    {
                        PostSystem("No active timeline to start.", ChatKind.Error);
                        return true;
                    }
                    FightTimeline.Instance.StartTimeline();
                    PostSystem($"Timeline '{FightTimeline.Instance.timelineName}' started.");
                    return true;

                // /help -> list available commands
                case "help":
                    PostSystem("Available commands:\n/clear (or /cl), /echo (or /e), /party (or /p), /time (or /t, /lt), /servertime (or /stime, /st), /start, /pause, /reset, /reload, /action (or /ac, /blueaction), /targetenemy (or /tenemy), /nexttarget (or /nt), /facetarget (or /ft), /enemysign (or /marking, /mark, /mk), /exit (or /quit, /close, /qqq)");
                    return true;

                // /exit, /quit, /close -> shutdown application
                case "exit":
                    PostSystem("Exiting application...");
                    Utilities.FunctionTimer.Create(FightTimeline.Instance, () => UnityEngine.Application.Quit(), 1f, "chat_command_shutdown", true, false);
                    return true;

                case "quit":
                    PostSystem("Exiting application...");
                    Utilities.FunctionTimer.Create(FightTimeline.Instance, () => UnityEngine.Application.Quit(), 1f, "chat_command_shutdown", true, false);
                    return true;

                case "close":
                    PostSystem("Exiting application...");
                    Utilities.FunctionTimer.Create(FightTimeline.Instance, () => UnityEngine.Application.Quit(), 1f, "chat_command_shutdown", true, false);
                    return true;

                case "qqq":
                    PostSystem("Exiting application...");
                    Utilities.FunctionTimer.Create(FightTimeline.Instance, () => UnityEngine.Application.Quit(), 1f, "chat_command_shutdown", true, false);
                    return true;

                default:
                    PostSystem($"Unknown command: /{cmd}", ChatKind.Error);
                    return true;
            }
        }

        private bool ActionCommand(CharacterState sender, List<CharacterState> targets, string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                PostSystem("Usage: /action <name> <target> OR /ac <name> <target>");
                return true;
            }

            // Extract actionName and targetSpec, supporting quoted action names like:
            // /ac "Action Name" <target>
            // /ac "Action Name" me
            string actionName;
            string targetSpec = null;

            if (args.StartsWith("\"", StringComparison.Ordinal))
            {
                int endQuote = args.IndexOf('"', 1);

                if (endQuote < 0)
                {
                    PostSystem("Invalid action name. Missing closing quote.", ChatKind.Error);
                    return true;
                }

                actionName = args.Substring(1, endQuote - 1).Trim();

                if (endQuote + 1 < args.Length)
                    targetSpec = args.Substring(endQuote + 1).Trim();
            }
            else
            {
                int spaceIdx = args.IndexOf(' ');

                if (spaceIdx < 0)
                {
                    actionName = args.Trim();
                }
                else
                {
                    actionName = args.Substring(0, spaceIdx).Trim();
                    targetSpec = args.Substring(spaceIdx + 1).Trim();
                }
            }

            if (sender == null || sender.actionController == null)
            {
                PostSystem("Cannot currently execute actions.", ChatKind.Error);
                return true;
            }

            if (!sender.actionController.TryGetAction(actionName, out CharacterAction result, StringComparison.OrdinalIgnoreCase, compareToDisplayName: true))
            {
                PostSystem($"Action '{actionName}' not found.", ChatKind.Error);
                return true;
            }

            CharacterState targetCharacter = null;
            bool useCurrentTarget = false;

            if (!string.IsNullOrWhiteSpace(targetSpec))
            {
                bool hadAngleBrackets = targetSpec.StartsWith("<") && targetSpec.EndsWith(">");

                // Remove wrapping <> or ""
                if (hadAngleBrackets)
                    targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);
                else if (targetSpec.StartsWith("\"") && targetSpec.EndsWith("\""))
                    targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);

                // Keep current target
                if ((targetSpec.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                     targetSpec.Equals("target", StringComparison.OrdinalIgnoreCase)))
                {
                    useCurrentTarget = true;
                }
                // Check if it's a number (index into targets list)
                else if (int.TryParse(targetSpec, out int indexAc))
                {
                    indexAc -= 1; // Convert to 0-based index for user-friendly input
                    indexAc = UnityEngine.Mathf.Clamp(indexAc, 0, targets.Count - 1); // Ensure non-negative index
                    if (targets != null && indexAc >= 0 && indexAc < targets.Count)
                        targetCharacter = targets[indexAc];
                }
                // Check if it's "me"
                else if (targetSpec.Equals("me", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter = sender;
                }
                // Try to find by character name
                else if (targets != null)
                {
                    CharacterState found = targets.Find(c => c.characterName.Equals(targetSpec, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                        targetCharacter = found;
                }

                if (!useCurrentTarget && targetCharacter == null)
                {
                    PostSystem("No valid target found.", ChatKind.Error);
                    return true;
                }
            }

            // No explicit target provided: keep existing behavior
            if (string.IsNullOrWhiteSpace(targetSpec) || useCurrentTarget)
            {
                if (result.Data.isTargeted && result.Data.requiresTarget && sender.targetController.currentTarget == null)
                {
                    PostSystem("Action requires a target.", ChatKind.Error);
                    return true;
                }

                sender.actionController.PerformAction(result);
                return true;
            }

            if (sender.targetController == null || targetCharacter?.targetController?.self == null)
            {
                PostSystem("Cannot currently set target for actions.", ChatKind.Error);
                return true;
            }

            sender.actionController.PerformAction(result, targetCharacter.targetController.self);

            return true;
        }

        private bool MarkCommand(CharacterState sender, List<CharacterState> targets, string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                PostSystem("Usage: /mark <markName> <target> OR /mk <markName> <target>");
                return true;
            }

            // Extract markName (first word) and targetSpec (rest)
            int spaceIdx = args.IndexOf(' ');
            string markName;
            string targetSpec = null;

            if (spaceIdx < 0)
            {
                markName = args.Trim();
            }
            else
            {
                markName = args.Substring(0, spaceIdx).Trim();
                targetSpec = args.Substring(spaceIdx + 1).Trim();
            }

            CharacterState targetCharacter = sender; // Default to sender

            if (!string.IsNullOrWhiteSpace(targetSpec))
            {
                // Remove wrapping <> or ""
                if (targetSpec.StartsWith("<") && targetSpec.EndsWith(">"))
                    targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);
                else if (targetSpec.StartsWith("\"") && targetSpec.EndsWith("\""))
                    targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);

                // Check if it's a number (index into targets list)
                if (int.TryParse(targetSpec, out int indexMk))
                {
                    if (targets != null && indexMk >= 0 && indexMk < targets.Count)
                        targetCharacter = targets[indexMk];
                }
                // Check if it's "me"
                else if (targetSpec.Equals("me", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter = sender;
                }
                // Try to find by character name
                else if (targets != null)
                {
                    CharacterState found = targets.Find(c => c.characterName.Equals(targetSpec, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                        targetCharacter = found;
                }
            }

            if (targetCharacter != null)
            {
                if (!markName.Equals("clear", StringComparison.OrdinalIgnoreCase) && !markName.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    PostSystem($"{sender.characterName} marked {targetCharacter.characterName} as {markName}");
                    targetCharacter.Mark(markName);
                }
                else
                {
                    PostSystem($"{sender.characterName} cleared marks from {targetCharacter.characterName}");
                    targetCharacter.Mark(string.Empty);
                }
            }
            else
            {
                PostSystem("No valid target found.", ChatKind.Error);
            }
            return true;
        }
    }
}