using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Scene]
    public string simSceneName = "demo";

#if UNITY_WEBPLAYER
    public const string webplayerQuitURL = "https://susy-bakaa.github.io/tools.html";
#endif
    Coroutine ie_exitApp;

#if UNITY_STANDALONE_WIN
    private void Start()
    {
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
#else
        Application.Quit();
#endif
    }
}
