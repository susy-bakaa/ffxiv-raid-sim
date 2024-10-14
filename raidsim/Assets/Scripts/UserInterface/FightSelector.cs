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

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        //Select(0);

#if UNITY_STANDALONE_WIN
        //Get the window handle.
        var windowPtr = FindWindow(null, "raidsim");
        //Set the title text using the window handle.
        SetWindowText(windowPtr, $"raidsim - {GetFightName()}");
#endif
    }

    void Update()
    {
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
        Utilities.FunctionTimer.Create(this, () => OnLoad(scene), loadDelay);
    }

    public void Load()
    {
        Utilities.FunctionTimer.Create(this, () => OnLoad(currentScene), loadDelay);
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
}