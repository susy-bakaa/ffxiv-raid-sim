// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;

namespace dev.susybaka.raidsim.Core
{
    public enum ChatKind { User, System, Log, Error }
    public enum ChatChannel { Echo, Party, System, Debug }

    public readonly struct ChatMessage
    {
        public readonly long Id;
        public readonly DateTime TimeUtc;
        public readonly string Sender;
        public readonly ChatKind Kind;
        public readonly ChatChannel Channel;
        public readonly string Text;

        public ChatMessage(long id, DateTime timeUtc, string sender, ChatKind kind, ChatChannel channel, string text)
        {
            Id = id;
            TimeUtc = timeUtc;
            Sender = sender;
            Kind = kind;
            Channel = channel;
            Text = text;
        }
    }
}