using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUserInterface : MonoBehaviour
{
    [SerializeField] private CanvasGroup fpsGroup;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        fpsGroup.alpha = 0f;

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains("-debug"))
            {
                fpsGroup.alpha = 1f;
            }
        }
    }
}