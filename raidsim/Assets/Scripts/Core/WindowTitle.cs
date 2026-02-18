// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace dev.susybaka.raidsim.Core
{
    public static class WindowTitle
    {
        /// <summary>
        /// Attempts to set the window title for the standalone application.
        /// Uses platform-specific APIs for Windows and Linux.
        /// </summary>
        /// <param name="title">The new title to set for the window.</param>
        /// <returns>True if the window title was successfully set; false if the operation failed or running in the editor.</returns>
        public static bool TrySet(string title)
        {
            if (Application.isEditor) return false;

            try
            {
#if UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
                var hwnd = FindWindow(null, GlobalVariables.lastWindowName);
                if (hwnd == IntPtr.Zero)
                {
                    Debug.LogWarning("Window handle not found. Skipping SetWindowText.");
                    return false;
                }
                SetWindowText(hwnd, title);
                GlobalVariables.lastWindowName = title;
                return true;
#elif UNITY_STANDALONE_LINUX && !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
                int ok = rsim_set_window_title(title);
                return ok != 0;
#endif
            }
            catch (DllNotFoundException e)
            {
                Debug.LogWarning($"Window title plugin not found: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting window title: {e}");
            }
            return false;
        }

        public static void ResetWindowCache()
        {
            if (Application.isEditor) return;

            try
            {
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
                rsim_reset_window_cache();
#endif
            }
            catch { /* keep it silent */ }
        }

#if UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string className, string windowName);
#endif

#if UNITY_STANDALONE_LINUX && !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
        [DllImport("rsim", EntryPoint = "rsim_set_window_title")]
        private static extern int rsim_set_window_title(string title);

        [DllImport("rsim", EntryPoint = "rsim_reset_window_cache")]
        private static extern void rsim_reset_window_cache();
#endif
    }
}