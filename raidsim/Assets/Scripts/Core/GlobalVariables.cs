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
        public static int versionNumber = 10;
        // Increment this when the scripts are changed in a way that will require a rebuild of all scenes inside bundles.
        // (This is not in use yet but added for future reference)
        public static int scriptingVersion = 1;
#if UNITY_EDITOR && !UNITY_EDITOR_LINUX
        public static string configPath = Application.dataPath + "/config.ini";
        public static string bgmPath = "F:/Git/ffxiv-raid-sim/raidsim/Source/Audio/bgm";
#elif UNITY_EDITOR_LINUX
        public static string configPath = Application.dataPath + "/config.ini";
        public static string bgmPath = "/mnt/ssd2/Git/ffxiv-raid-sim/raidsim/Source/Audio/bgm";
#elif PLATFORM_STANDALONE_WIN && !PLATFORM_STANDALONE_LINUX
        public static string configPath = Path.GetDirectoryName(Application.dataPath) + "/config.ini";
        public static string bgmPath = Path.GetDirectoryName(Application.dataPath) + "/bgm";
#else
        public static string configPath = Application.persistentDataPath + "/config.ini";
        public static string bgmPath = Application.persistentDataPath + "/bgm";
#endif
#if UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_EDITOR_LINUX
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
        public static readonly Color allyDefaultColor = new Color(0.502f, 0.929f, 1); // "#0065FF" pale cyan
        public static readonly Color enemyDefaultColor = new Color(1f, 0.494f, 0.584f); // "#FF7E95" pale red
        public static readonly string[] botRoleNames = { "Tank 1", "Tank 2", "Healer 1", "Healer 2", "Melee 1", "Melee 2", "Ranged 1", "Ranged 2" };
        public static readonly string[] botRoleEasternShortNames = { "MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4" };
        public static readonly string[] botRoleWesternShortNames = { "MT", "OT", "H1", "H2", "M1", "M2", "R1", "R2" };
        public static readonly Color[] botRoleColors = { new Color(0f, 0.396f, 1f), new Color(0f, 0.396f, 1f), new Color(0.337f, 0.827f, 0.173f), new Color(0.337f, 0.827f, 0.173f), new Color(0.89f, 0.38f, 0.38f), new Color(0.89f, 0.38f, 0.38f), new Color(0.89f, 0.38f, 0.38f), new Color(0.89f, 0.38f, 0.38f) }; //{ "#0065FF", "#0065FF", "#56D32C", "#56D32C", "#DB4E4E", "#DB4E4E", "#DB4E4E", "#DB4E4E" };
    }
}