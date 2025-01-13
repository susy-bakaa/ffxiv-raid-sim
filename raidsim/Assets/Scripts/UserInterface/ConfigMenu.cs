using System;
using System.Collections;
using System.Collections.Generic;
using no00ob.WaveSurvivalGame.UserInterface;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static PlayerController;

public class ConfigMenu : MonoBehaviour
{
    CanvasGroup group;
    PlayerController playerController;
    TargetController playerTargeting;
    ThirdPersonCamera thirdPersonCamera;
    UserInput userInput;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private SliderInputLinker scaleSliderSync;
    [SerializeField] private ToggleLinker toggleMovement;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private ResolutionDropdown resolutionDropdown;
    [SerializeField] private SliderInputLinker volumeSliderSync;
    [SerializeField] private Toggle invertVerticalCameraToggle;
    [SerializeField] private Toggle invertHorizontalCameraToggle;
    [SerializeField] private TMP_Dropdown cameraAdjustmentDropdown;
    float scale = 50;
    float newScale = 50;
    bool legacy = true;
    bool newLegacy = true;
    bool fullscreen = true;
    bool newFullscreen = true;
    int resolution = 0;
    int newResolution = 0;
    float volume = 100;
    float newVolume = 100;
    bool invertCameraVertical = false;
    bool newInvertCameraVertical = false;
    bool invertCameraHorizontal = false;
    bool newInvertCameraHorizontal = false;
    int cameraAdjustment = 0;
    int newCameraAdjustment = 0;
    [SerializeField] private CanvasGroup applyPopup;
    public bool isOpen;
    public bool isApplyPopupOpen;

    bool menuVisible = false;
    bool applyPopupVisible = false;
    bool configSaved = true;
    bool start = false;

    void Awake()
    {
        userInput = FindObjectOfType<UserInput>();
        group = GetComponent<CanvasGroup>();
        playerController = FindObjectOfType<PlayerController>();
        thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
        if (playerController != null && playerTargeting == null)
            playerTargeting = playerController.gameObject.GetComponent<TargetController>();

        if (playerTargeting != null)
            playerTargeting.SetConfigMenu(this);

        menuVisible = false;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        applyPopupVisible = false;
        applyPopup.alpha = 0f;
        applyPopup.blocksRaycasts = false;
        applyPopup.interactable = false;
    }

    void Update()
    {
        if (userInput != null)
        {
            if (isOpen)
            {
                userInput.inputEnabled = false;
                userInput.zoomInputEnabled = false;
                userInput.movementInputEnabled = false;
                userInput.rotationInputEnabled = false;
                userInput.targetRaycastInputEnabled = false;
            }
        }

        if (!start)
        {
            start = true;
            scale = scaleSliderSync.Slider.value;
            legacy = toggleMovement.toggles[0].isOn;
            newLegacy = legacy;
            if (playerController != null)
                playerController.legacyMovement = legacy;
            volume = volumeSliderSync.Slider.value;
        }
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
            userInput.inputEnabled = true;
            userInput.zoomInputEnabled = true;
            userInput.movementInputEnabled = true;
            userInput.rotationInputEnabled = true;
            userInput.targetRaycastInputEnabled = true;
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
        ChangeHUDScale((float)scale);
    }

    public void ChangeHUDScale(float scale)
    {
        configSaved = false;
        newScale = scale;
    }

    public void ChangeAudioVolume(string volume)
    {
        if (float.TryParse(volume, out float result))
        {
            ChangeAudioVolume(result);
        }
    }

    public void ChangeAudioVolume(int volume)
    {
        ChangeAudioVolume((float)volume);
    }

    public void ChangeAudioVolume(float volume)
    {
        configSaved = false;
        newVolume = volume;
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

    public void ChangeInvertVerticalCamera(bool value)
    {
        configSaved = false;
        newInvertCameraVertical = value;
    }

    public void ChangeInvertHorizontalCamera(bool value)
    {
        configSaved = false;
        newInvertCameraHorizontal = value;
    }

    public void ChangeCameraAutoAdjustment(int value)
    {
        configSaved = false;
        newCameraAdjustment = value;
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
        volume = newVolume;
        if (volume < 1)
        {
            volume = 0.001f;
        }
        audioMixer.SetFloat("Volume", Mathf.Log10(volume / 100) * 20f);
        invertCameraVertical = newInvertCameraVertical;
        invertCameraHorizontal = newInvertCameraHorizontal;
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.invertY = invertCameraVertical;
            thirdPersonCamera.invertX = invertCameraHorizontal;
        }
        cameraAdjustment = newCameraAdjustment;
        if (playerController != null)
            playerController.autoAdjustCamera = (CameraAdjustMode)cameraAdjustment;
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
        newVolume = 100f;
        volumeSliderSync.Slider.value = newVolume;
        volumeSliderSync.Sync(0);
        newInvertCameraVertical = false;
        newInvertCameraHorizontal = false;
        thirdPersonCamera.invertY = newInvertCameraVertical;
        thirdPersonCamera.invertX = newInvertCameraHorizontal;
        newCameraAdjustment = 0;
        if (playerController != null)
            playerController.autoAdjustCamera = CameraAdjustMode.Moving;
        ApplySettings();
#endif
    }

    public void CancelSettings()
    {
        newScale = scale;
        scaleSliderSync.Slider.value = newScale;
        scaleSliderSync.Sync(0);
        newLegacy = legacy;
        //Debug.Log($"newLegacy {newLegacy} legacy {legacy}");
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
        newVolume = volume;
        volumeSliderSync.Slider.value = newVolume;
        volumeSliderSync.Sync(0);
        newInvertCameraVertical = invertCameraVertical;
        newInvertCameraHorizontal = invertCameraHorizontal;
        invertVerticalCameraToggle.SetIsOnWithoutNotify(newInvertCameraVertical);
        invertHorizontalCameraToggle.SetIsOnWithoutNotify(newInvertCameraHorizontal);
        newCameraAdjustment = cameraAdjustment;
        cameraAdjustmentDropdown.SetValueWithoutNotify(newCameraAdjustment);
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
