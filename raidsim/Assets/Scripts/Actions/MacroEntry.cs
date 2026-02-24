// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;

namespace dev.susybaka.raidsim.Actions
{
    public enum MacroIconMode
    {
        Default,
        CustomSprite,
        ActionIcon // /micon or /macroicon ActionId
    }

    [Serializable]
    public struct MacroEntry
    {
        public bool isValid;
        public string name;
        [TextArea(1, 15)] public string body;

        public MacroIconMode iconMode;

        // For CustomSprite: key into your icon DB
        public string customIconId;

        // For ActionIcon: an ActionId (must exist in registry)
        public string actionIconId;
    }
}