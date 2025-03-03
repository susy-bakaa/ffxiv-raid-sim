using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DebugUserInterface : MonoBehaviour
{
    [SerializeField] private AssetHandler assetHandler;
    [SerializeField] private CanvasGroup fpsGroup;
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private TextMeshProUGUI screenResolutionText;
    [SerializeField] private TextMeshProUGUI fullscreenText;
    [SerializeField] private TextMeshProUGUI screenModeText;
    [SerializeField] private GameObject thirdpartyDebugInterface;
    [SerializeField, HideIf("showDebug")] private bool showFPS = false;
    [SerializeField, HideIf("showDebug")] private bool showScreen = false;
    [SerializeField, HideIf("showDebug")] private bool showInfo = false;
    [SerializeField] private bool showDebug = false;
    private bool keyPress = false;

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
        thirdpartyDebugInterface.gameObject.SetActive(false);

#if !UNITY_EDITOR
        showDebug = false;
        showFPS = false;
        showScreen = false;
        showInfo = false;
#endif

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains("-strip"))
            {
                if (assetHandler != null)
                {
                    assetHandler.disable = true;
                }
            }
            if (args[i].Contains("-debug") || showDebug)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                thirdpartyDebugInterface.gameObject.SetActive(true);
            }
            else if (args[i].Contains("-info") || showInfo)
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
                if (args[i].Contains("-fps") || showFPS)
                {
                    fpsGroup.gameObject.SetActive(true);
                    fpsGroup.alpha = 1f;
                }
                if (args[i].Contains("-screen") || showScreen)
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home) && !keyPress)
        {
            keyPress = true;
            showDebug = !showDebug;

            if (showDebug)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                thirdpartyDebugInterface.gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
                thirdpartyDebugInterface.gameObject.SetActive(false);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Home))
        {
            keyPress = false;
        }
    }
}