using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GlobalVariables;

public class FightSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;
    public Button loadButton;
    public float loadDelay = 2f;
    public List<string> scenes = new List<string>();
    public string currentScene;

    private Coroutine ieLoadSceneDelayed;

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        //Select(0);

        if (Application.isEditor)
            return;

#if UNITY_STANDALONE_WIN
        try
        {
            var windowPtr = FindWindow(null, GlobalVariables.lastWindowName);
            if (windowPtr == IntPtr.Zero)
            {
                Debug.LogError("Window handle not found. Skipping SetWindowText.");
            }
            else
            {
                string windowName = $"raidsim - {GetFightName()}";
                SetWindowText(windowPtr, windowName);
                GlobalVariables.lastWindowName = windowName;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting window title: {ex}");
        }
#endif
    }

    void Update()
    {
        if (FightTimeline.Instance == null)
            return;

        dropdown.interactable = !FightTimeline.Instance.playing;
        loadButton.interactable = !FightTimeline.Instance.playing;
    }

    public void Select(int value)
    {
        if (scenes.Count < (value + 1))
        {
            Debug.Log("Fight not implemented yet.");
            return;
        }
        currentScene = scenes[value];
    }

    public void Reload(string scene)
    {
        if (ieLoadSceneDelayed == null)
        {
            ieLoadSceneDelayed = StartCoroutine(IE_LoadSceneDelayed(scene, new WaitForSeconds(loadDelay)));
        }
    }

    public void Load()
    {
        if (ieLoadSceneDelayed == null)
        {
            ieLoadSceneDelayed = StartCoroutine(IE_LoadSceneDelayed(currentScene, new WaitForSeconds(loadDelay)));
        }
    }

    private void OnLoad(string scene)
    {
        //Utilities.FunctionTimer.CleanUp();
        SceneManager.LoadScene(scene);
    }

    private string GetFightName()
    {
        return FightTimeline.Instance.timelineName;
    }

    private IEnumerator IE_LoadSceneDelayed(string scene, WaitForSeconds wait)
    {
        yield return wait;
        ieLoadSceneDelayed = null;
        OnLoad(scene);
    }
}