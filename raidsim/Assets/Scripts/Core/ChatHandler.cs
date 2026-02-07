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
        private readonly Dictionary<string, string> _macros = new(StringComparer.OrdinalIgnoreCase);

        private long _nextId = 1;
        private int _maxHistory = 500;

        public int MaxHistory
        {
            get => _maxHistory;
            set => _maxHistory = Math.Max(50, value);
        }

        private ChatHandler() { }

        public void PostUser(CharacterState sender, string text, ChatChannel channel = ChatChannel.Party)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            string name = sender != null ? sender.characterName : "User";

            // commands/macros start with "/"
            string trimmedText = text.Trim();
            if (TryExecuteCommand(sender, trimmedText, ref channel))
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

        private bool TryExecuteCommand(CharacterState sender, string raw, ref ChatChannel channel)
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

                // /note this is a thing to remember
                case "note":
                    PostSystem(string.IsNullOrWhiteSpace(args) ? "Usage: /note <text>" : $"NOTE: {args}");
                    return true;

                // /macro set name some text
                // /macro name  (exec)
                case "macro":
                {
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        PostSystem("Usage: /macro set <name> <text>  OR  /macro <name>");
                        return true;
                    }

                    parts = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && parts[0].Equals("set", StringComparison.OrdinalIgnoreCase))
                    {
                        if (parts.Length < 3)
                        {
                            PostSystem("Usage: /macro set <name> <text>");
                            return true;
                        }

                        var name = parts[1];
                        var body = parts[2];
                        _macros[name] = body;
                        PostSystem($"Macro '{name}' saved.");
                        return true;
                    }
                    else
                    {
                        var name = parts[0];
                        if (_macros.TryGetValue(name, out var body))
                        {
                            // For now: macros become system messages. Later: broadcast to party/raid.
                            PostSystem(body);
                        }
                        else
                        {
                            PostSystem($"Macro '{name}' not found.");
                        }
                        return true;
                    }
                }

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
                    PostSystem("Available commands:\n/clear, /echo (or /e), /party (or /p), /note, /macro, /time (or /t, /lt), /servertime (or /stime, /st), /start, /pause, /reset, /reload, /action (or /ac), /exit (or /quit, /close)");
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