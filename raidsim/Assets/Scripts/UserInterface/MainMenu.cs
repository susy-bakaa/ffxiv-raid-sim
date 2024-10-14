using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
#if UNITY_WEBPLAYER
    public static string webplayerQuitURL = "http://google.com";
#endif
    Coroutine ie_exitApp;

    public void LoadSimScene()
    {
        //Utilities.FunctionTimer.CleanUp();
        SceneManager.LoadScene("demo");
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
