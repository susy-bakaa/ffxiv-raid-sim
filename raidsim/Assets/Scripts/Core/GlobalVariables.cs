using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class GlobalVariables
{
#if UNITY_EDITOR
    public static string configPath = Application.dataPath + "/config.ini";
#else
    public static string configPath = Path.GetDirectoryName(Application.dataPath) + "/config.ini";
#endif
}
