// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace dev.susybaka.raidsim.Core
{
    public static class GlobalVariables
    {
        // This value is incremented when the game gets an update.
        // It is used by WebGL to determine if the game assets need to be redownloaded and by the auto updater for version checks for updates.
        public static int versionNumber = 9;
        // Increment this when the scripts are changed in a way that will require a rebuild of all scenes inside bundles.
        // (This is not in use yet but added for future reference)
        public static int scriptingVersion = 1;
#if UNITY_EDITOR
        public static string configPath = Application.dataPath + "/config.ini";
        public static string bgmPath = "F:/Git/ffxiv-raid-sim/raidsim/Source/Audio/bgm";
#elif PLATFORM_STANDALONE_WIN
        public static string configPath = Path.GetDirectoryName(Application.dataPath) + "/config.ini";
        public static string bgmPath = Path.GetDirectoryName(Application.dataPath) + "/bgm";
#else
        public static string configPath = Application.persistentDataPath + "/config.ini";
        public static string bgmPath = Application.persistentDataPath + "/bgm";
#endif
#if UNITY_STANDALONE_WIN
        //Import the following.
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
        public static string lastWindowName = "raidsim";
#endif
#if UNITY_EDITOR
        public static bool muteBgm = true;
#else
        public static bool muteBgm = false;
#endif
        public static Vector3 worldBounds = new Vector3(1000, 1000, 1000);
        public static Resolution currentGameResolution; // Cached resolution for comparison
        public const long maximumDamage = 999999999999999999;
        public static Vector3 minimapZoom;
        public static bool modifiedMinimapZoom = false;
        public static bool rotateMinimap = false;
        public const string assetBundleExtension = ".pak";
    }
}