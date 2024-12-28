using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WEBPLAYER
using System.Runtime.InteropServices;
#endif
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Scene]
    public string simSceneName = "demo";

#if UNITY_WEBPLAYER
    public const string webplayerQuitURL = "https://susy-bakaa.github.io/tools.html";
    [DllImport("__Internal")]
    private static extern void closewindow();
#endif
    Coroutine ie_exitApp;

#if UNITY_STANDALONE_WIN
    private void Start()
    {
        KeyBind.SetupKeyNames();

        if (Application.isEditor)
            return;

        try
        {
            var windowPtr = GlobalVariables.FindWindow(null, GlobalVariables.lastWindowName);
            if (windowPtr == IntPtr.Zero)
            {
                Debug.LogError("Window handle not found. Skipping SetWindowText.");
            }
            else
            {
                string windowName = "raidsim";
                GlobalVariables.SetWindowText(windowPtr, windowName);
                GlobalVariables.lastWindowName = windowName;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting window title: {ex}");
        }
    }
#endif

    public void LoadSimScene()
    {
        //Utilities.FunctionTimer.CleanUp();
        SceneManager.LoadScene(simSceneName);
        StopAllCoroutines();
    }

    public void Quit()
    {
        if (ie_exitApp == null)
            ie_exitApp = StartCoroutine(IE_ExitApp());
    }

    private IEnumerator IE_ExitApp()
    {
        yield return new WaitForSecondsRealtime(0.66f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
        Application.Quit();
        closewindow();
#else
        Application.Quit();
#endif
    }
}
