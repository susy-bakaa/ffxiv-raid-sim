using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public static class GlobalVariables
{
#if UNITY_EDITOR
    public static string configPath = Application.dataPath + "/config.ini";
#else
    public static string configPath = Path.GetDirectoryName(Application.dataPath) + "/config.ini";
#endif
#if UNITY_STANDALONE_WIN
    //Import the following.
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
#endif
}
