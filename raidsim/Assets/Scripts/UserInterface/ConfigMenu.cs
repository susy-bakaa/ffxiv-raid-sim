using System;
using System.Collections;
using System.Collections.Generic;
using no00ob.WaveSurvivalGame.UserInterface;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour
{
    CanvasGroup group;
    PlayerController playerController;
    TargetController playerTargeting;

    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private SliderInputLinker scaleSliderSync;
    [SerializeField] private ToggleLinker toggleMovement;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private ResolutionDropdown resolutionDropdown;
    float scale;
    float newScale;
    bool legacy;
    bool newLegacy;
    bool fullscreen;
    bool newFullscreen;
    int resolution;
    int newResolution;
    [SerializeField] private CanvasGroup applyPopup;
    public bool isOpen;
    public bool isApplyPopupOpen;

    bool menuVisible = false;
    bool applyPopupVisible = false;
    bool configSaved = true;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null && playerTargeting == null)
            playerTargeting = playerController.gameObject.GetComponent<TargetController>();

        if (playerTargeting != null)
            playerTargeting.SetConfigMenu(this);

        scale = scaleSliderSync.Slider.value;
        legacy = toggleMovement.toggles[0].isOn;

        menuVisible = false;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        applyPopupVisible = false;
        applyPopup.alpha = 0f;
        applyPopup.blocksRaycasts = false;
        applyPopup.interactable = false;
    }

    public void ToggleConfigMenu()
    {
        menuVisible = !menuVisible;

        if (menuVisible)
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
            isOpen = true;
        }
        else
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            isOpen = false;
        }
    }

    public void ToggleApplyPopup(bool state)
    {
        applyPopupVisible = state;

        if (applyPopupVisible)
        {
            applyPopup.alpha = 1f;
            applyPopup.blocksRaycasts = true;
            applyPopup.interactable = true;
            isApplyPopupOpen = true;
        }
        else
        {
            applyPopup.alpha = 0f;
            applyPopup.blocksRaycasts = false;
            applyPopup.interactable = false;
            isApplyPopupOpen = false;
        }
    }

    public void ToggleApplyPopup()
    {
        applyPopupVisible = !applyPopupVisible;

        ToggleApplyPopup(applyPopupVisible);
    }

    public void ChangeHUDScale(string scale)
    {
        if (float.TryParse(scale, out float result))
        {
            ChangeHUDScale(result);
        }
    }

    public void ChangeHUDScale(int scale)
    {
        ChangeHUDScale(scale);
    }

    public void ChangeHUDScale(float scale)
    {
        configSaved = false;
        newScale = scale;
    }

    public void ChangeMovementType(bool value)
    {
        configSaved = false;
        newLegacy = value;
    }

    public void ChangeFullscreenMode(bool value)
    {
#if UNITY_WEBPLAYER
        return;
#else
        configSaved = false;
        newFullscreen = value;
#endif
    }

    public void ChangeResolution(Int32 resolutionIndex)
    {
#if UNITY_WEBPLAYER
        return;
#else
        configSaved = false;
        newResolution = resolutionIndex;
#endif
    }

    public void ApplySettings()
    {
        if (Mathf.Approximately(newScale, 50f))
        {
            scale = 50f;
            canvasScaler.scaleFactor = 1f;
        }
        else
        {
            scale = newScale;
            canvasScaler.scaleFactor = Utilities.Map(newScale, 0, 100, 0.5f, 2f);
        }
        legacy = newLegacy;
        if (playerController != null)
            playerController.legacyMovement = newLegacy;
#if UNITY_WEBPLAYER
        configSaved = true;
#else
        fullscreen = newFullscreen;
        Screen.fullScreen = fullscreen;
        if (Screen.fullScreen)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
        resolution = newResolution;
        resolutionDropdown.SetResolution(resolution, Screen.fullScreen);
        configSaved = true;
#endif
    }

    public void CloseSettings()
    {
        if (!configSaved)
        {
            ToggleApplyPopup(true);
        }
        else
        {
            ToggleApplyPopup(false);
            ToggleConfigMenu();
        }
    }

    public void DefaultSettings()
    {
        newScale = 50f;
        scaleSliderSync.Slider.value = newScale;
        scaleSliderSync.Sync(0);
        newLegacy = true;
        toggleMovement.toggles[0].SetIsOnWithoutNotify(true);
        toggleMovement.toggles[1].SetIsOnWithoutNotify(false);
#if UNITY_WEBPLAYER
        ApplySettings();
        return;
#else
        newFullscreen = true;
        newResolution = 0;
        ApplySettings();
#endif
    }

    public void CancelSettings()
    {
        newScale = scale;
        scaleSliderSync.Slider.value = newScale;
        scaleSliderSync.Sync(0);
        newLegacy = legacy;
        Debug.Log($"newLegacy {newLegacy} legacy {legacy}");
        toggleMovement.toggles[0].SetIsOnWithoutNotify(newLegacy);
        toggleMovement.toggles[1].SetIsOnWithoutNotify(!newLegacy);
#if UNITY_WEBPLAYER
        ApplySettings();
        return;
#else
        newFullscreen = fullscreen;
        fullscreenToggle.SetIsOnWithoutNotify(newFullscreen);
        newResolution = resolution;
        resolutionDropdown.SetResolutionWithoutNotify(newResolution, newFullscreen);
        ApplySettings();
#endif
    }

#if UNITY_EDITOR
    private static EditorWindow GetGameView()
    {
        // This returns the GameView EditorWindow
        var assembly = typeof(EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        return EditorWindow.GetWindow(type);
    }
#endif
}
