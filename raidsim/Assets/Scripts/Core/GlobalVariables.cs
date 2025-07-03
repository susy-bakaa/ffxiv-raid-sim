using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class GlobalVariables
{
    // This value is incremented when the game gets an updated.
    // It is used by WebGL to determine if the game assets need to be redownloaded and by the auto updater for version checks for updates.
    public const int versionNumber = 5;
#if UNITY_EDITOR
    public static string configPath = Application.dataPath + "/config.ini";
    public static string bgmPath = "F:/Users/Aki/Files/GitHub/ffxiv-raid-sim/raidsim/Source/Audio/bgm";
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
    public static bool muteBgm = false;
    public static Vector3 worldBounds = new Vector3(1000, 1000, 1000);
    public static Resolution currentGameResolution; // Cached resolution for comparison
    public const long maximumDamage = 999999999999999999;
    public static Vector3 minimapZoom;
    public static bool modifiedMinimapZoom = false;
    public static bool rotateMinimap = false;
}
