using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
#if UNITY_WEBPLAYER
    public static string webplayerQuitURL = "http://google.com";
#endif

    public void LoadSimScene()
    {
        SceneManager.LoadScene("helloworld1");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
#else
        Application.Quit();
#endif
    }
}
