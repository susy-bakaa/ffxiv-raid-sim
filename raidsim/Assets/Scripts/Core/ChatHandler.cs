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
            string[] parts;

            if (cmd.Length == 0)
                return true;

            switch (cmd.ToLowerInvariant())
            {
                case "clear":
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

                case "mark":
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
                    
                    CharacterState targetChar = sender; // Default to sender
                    
                    if (!string.IsNullOrWhiteSpace(targetSpec))
                    {
                        // Remove wrapping <> or ""
                        if (targetSpec.StartsWith("<") && targetSpec.EndsWith(">"))
                            targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);
                        else if (targetSpec.StartsWith("\"") && targetSpec.EndsWith("\""))
                            targetSpec = targetSpec.Substring(1, targetSpec.Length - 2);
                        
                        // Check if it's a number (index into targets list)
                        if (int.TryParse(targetSpec, out int index))
                        {
                            if (targets != null && index >= 0 && index < targets.Count)
                                targetChar = targets[index];
                        }
                        // Check if it's "me"
                        else if (targetSpec.Equals("me", StringComparison.OrdinalIgnoreCase))
                        {
                            targetChar = sender;
                        }
                        // Try to find by character name
                        else if (targets != null)
                        {
                            CharacterState found = targets.Find(c => c.characterName.Equals(targetSpec, StringComparison.OrdinalIgnoreCase));
                            if (found != null)
                                targetChar = found;
                        }
                    }
                    
                    if (targetChar != null)
                    {
                        if (!markName.Equals("clear", StringComparison.OrdinalIgnoreCase) && !markName.Equals("off", StringComparison.OrdinalIgnoreCase))
                        {
                            PostSystem($"{sender.characterName} marked {targetChar.characterName} as {markName}");
                            targetChar.Mark(markName);
                        }
                        else
                        {
                            PostSystem($"{sender.characterName} cleared marks from {targetChar.characterName}");
                            targetChar.Mark(string.Empty);
                        }
                    }
                    else
                    {
                        PostSystem("No valid target found.", ChatKind.Error);
                    }
                    return true;
                
                case "mk":
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        PostSystem("Usage: /mark <markName> <target> OR /mk <markName> <target>");
                        return true;
                    }
                    
                    // Extract markName (first word) and targetSpec (rest)
                    int spaceIdxMk = args.IndexOf(' ');
                    string markNameMk;
                    string targetSpecMk = null;
                    
                    if (spaceIdxMk < 0)
                    {
                        markNameMk = args.Trim();
                    }
                    else
                    {
                        markNameMk = args.Substring(0, spaceIdxMk).Trim();
                        targetSpecMk = args.Substring(spaceIdxMk + 1).Trim();
                    }
                    
                    CharacterState targetCharMk = sender; // Default to sender
                    
                    if (!string.IsNullOrWhiteSpace(targetSpecMk))
                    {
                        // Remove wrapping <> or ""
                        if (targetSpecMk.StartsWith("<") && targetSpecMk.EndsWith(">"))
                            targetSpecMk = targetSpecMk.Substring(1, targetSpecMk.Length - 2);
                        else if (targetSpecMk.StartsWith("\"") && targetSpecMk.EndsWith("\""))
                            targetSpecMk = targetSpecMk.Substring(1, targetSpecMk.Length - 2);
                        
                        // Check if it's a number (index into targets list)
                        if (int.TryParse(targetSpecMk, out int indexMk))
                        {
                            if (targets != null && indexMk >= 0 && indexMk < targets.Count)
                                targetCharMk = targets[indexMk];
                        }
                        // Check if it's "me"
                        else if (targetSpecMk.Equals("me", StringComparison.OrdinalIgnoreCase))
                        {
                            targetCharMk = sender;
                        }
                        // Try to find by character name
                        else if (targets != null)
                        {
                            CharacterState foundMk = targets.Find(c => c.characterName.Equals(targetSpecMk, StringComparison.OrdinalIgnoreCase));
                            if (foundMk != null)
                                targetCharMk = foundMk;
                        }
                    }
                    
                    if (targetCharMk != null)
                    {
                        if (!markNameMk.Equals("clear", StringComparison.OrdinalIgnoreCase) && !markNameMk.Equals("off", StringComparison.OrdinalIgnoreCase))
                        {
                            PostSystem($"{sender.characterName} marked {targetCharMk.characterName} as {markNameMk}");
                            targetCharMk.Mark(markNameMk);
                        }
                        else
                        {
                            PostSystem($"{sender.characterName} cleared marks from {targetCharMk.characterName}");
                            targetCharMk.Mark(string.Empty);
                        }
                    }
                    else
                    {
                        PostSystem("No valid target found.", ChatKind.Error);
                    }
                    return true;

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
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        PostSystem("Usage: /action <name> OR /ac <name>");
                        return true;
                    }

                    parts = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 1)
                    {
                        string actionName = parts[0].ToLower();

                        if (sender != null && sender.actionController != null)
                        {
                            if (sender.actionController.TryGetAction(actionName, out CharacterAction result, StringComparison.OrdinalIgnoreCase))
                            {
                                sender.actionController.PerformAction(result);
                                return true;
                            }
                            else
                            {
                                PostSystem($"Action '{actionName}' not found.", ChatKind.Error);
                                return true;
                            }
                        }
                        else
                        {
                            PostSystem("Cannot currently execute actions.", ChatKind.Error);
                            return true;
                        }
                    }
                    else
                    {
                        PostSystem("Usage: /action <name> OR /ac <name>");
                        return true;
                    }
                case "ac":
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        PostSystem("Usage: /action <name> OR /ac <name>");
                        return true;
                    }

                    parts = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 1)
                    {
                        string actionName = parts[0].ToLower();

                        if (sender != null && sender.actionController != null)
                        {
                            if (sender.actionController.TryGetAction(actionName, out CharacterAction result, StringComparison.OrdinalIgnoreCase))
                            {
                                sender.actionController.PerformAction(result);
                                return true;
                            }
                            else
                            {
                                PostSystem($"Action '{actionName}' not found.", ChatKind.Error);
                                return true;
                            }
                        }
                        else
                        {
                            PostSystem("Cannot currently execute actions.", ChatKind.Error);
                            return true;
                        }
                    }
                    else
                    {
                        PostSystem("Usage: /action <name> OR /ac <name>");
                        return true;
                    }

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
                    PostSystem("Available commands:\n/clear, /echo (or /e), /party (or /p), /time (or /t, /lt), /servertime (or /stime, /st), /start, /pause, /reset, /reload, /action (or /ac), /mark (or /mk), /exit (or /quit, /close)");
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

                default:
                    PostSystem($"Unknown command: /{cmd}", ChatKind.Error);
                    return true;
            }
        }
    }
}