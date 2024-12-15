using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHandler : MonoBehaviour
{
    public bool disableFogForWebGL = true;
    public bool disableFogForWindows = false;
    public bool disableFogForLinux = false;

    private void Awake()
    {
#if UNITY_WEBPLAYER
        if (disableFogForWebGL)
        {
            RenderSettings.fog = false;
        }
#endif
#if UNITY_STANDALONE_WIN
        if (disableFogForWindows)
        {
            RenderSettings.fog = false;
        }
#endif
#if UNITY_STANDALONE_LINUX
        if (disableFogForLinux)
        {
            RenderSettings.fog = false;
        }
#endif
    }
}
