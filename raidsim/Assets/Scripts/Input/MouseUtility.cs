// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.InputSystem;

namespace dev.susybaka.raidsim.Inputs
{
    public static class MouseUtility
    {
        public static Vector2 StoredMousePosition { get; private set; }

        public static void StoreMousePosition()
        {
            if (Mouse.current == null)
                return;

            StoredMousePosition = Mouse.current.position.ReadValue();
        }

        public static void RestoreMousePosition()
        {
            if (Mouse.current == null)
                return;

            Mouse.current.WarpCursorPosition(StoredMousePosition);
        }

        public static void SetMousePosition(Vector2 position)
        {
            if (Mouse.current == null)
                return;

            Mouse.current.WarpCursorPosition(position);
        }
    }
}