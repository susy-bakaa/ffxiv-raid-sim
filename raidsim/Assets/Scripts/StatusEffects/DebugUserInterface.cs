using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUserInterface : MonoBehaviour
{
    [SerializeField] private CanvasGroup fpsGroup;
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private TextMeshProUGUI screenResolutionText;
    [SerializeField] private TextMeshProUGUI fullscreenText;
    [SerializeField] private TextMeshProUGUI screenModeText;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        fpsGroup.alpha = 0f;
        fpsGroup.gameObject.SetActive(false);
        resolutionText.alpha = 0f;
        screenResolutionText.gameObject.SetActive(false);
        screenResolutionText.alpha = 0f;
        fullscreenText.gameObject.SetActive(false);
        fullscreenText.alpha = 0f;
        screenModeText.gameObject.SetActive(false);
        screenModeText.alpha = 0f;
        screenModeText.gameObject.SetActive(false);

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains("-debug"))
            {
                fpsGroup.gameObject.SetActive(true);
                fpsGroup.alpha = 1f;
                resolutionText.gameObject.SetActive(true);
                resolutionText.alpha = 1f;
                screenResolutionText.gameObject.SetActive(true);
                screenResolutionText.alpha = 1f;
                fullscreenText.gameObject.SetActive(true);
                fullscreenText.alpha = 1f;
                screenModeText.gameObject.SetActive(true);
                screenModeText.alpha = 1f;
            }
            else
            {
                if (args[i].Contains("-fps"))
                {
                    fpsGroup.gameObject.SetActive(true);
                    fpsGroup.alpha = 1f;
                }
                if (args[i].Contains("-screen"))
                {
                    resolutionText.gameObject.SetActive(true);
                    resolutionText.alpha = 1f;
                    screenResolutionText.gameObject.SetActive(true);
                    screenResolutionText.alpha = 1f;
                    fullscreenText.gameObject.SetActive(true);
                    fullscreenText.alpha = 1f;
                    screenModeText.gameObject.SetActive(true);
                    screenModeText.alpha = 1f;
                }
            }
        }
    }
}