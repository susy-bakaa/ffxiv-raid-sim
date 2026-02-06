// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.Targeting;
using dev.susybaka.Shared;
using dev.susybaka.Shared.UserInterface;
using dev.susybaka.WaveSurvivalGame.UserInterface;
using static dev.susybaka.raidsim.Characters.PlayerController;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI
{
    public class ConfigMenu : HudWindow
    {
        PlayerController playerController;
        TargetController playerTargeting;
        ThirdPersonCamera thirdPersonCamera;
        UserInput userInput;
        FightTimeline timeline;
        SpotSelector spotSelector;
        RoleSelector roleSelector;

        [Header("Config Menu")]
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
        [SerializeField] private SliderInputLinker cameraSensitivitySync;
        [SerializeField] private ToggleLinker toggleMouseCursorType;
        [SerializeField] private Toggle skipUpdatesToggle;
        [SerializeField] private TMP_Dropdown botNameTypeDropdown;
        [SerializeField] private Toggle botNameRoleColorsToggle;
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
        float cameraSensitivity = 10f;
        float newCameraSensitivity = 10f;
        bool hardwareCursor = true;
        bool newHardwareCursor = true;
        bool skipUpdates = false;
        bool newSkipUpdates = false;
        int botNameType = 0;
        int newBotNameType = 0;
        bool botNameRoleColors = false;
        bool newBotNameRoleColors = false;
        [SerializeField] private HudWindow applyPopup;
        public HudWindow ApplyPopup { get { return applyPopup; } }

        bool menuVisible = false;
        bool applyPopupVisible = false;
        bool configSaved = true;
        bool start = false;
        float originalMouseSensitivity;
        float originalControllerSensitivity;
        float originalRotationSpeed;

        protected override void Awake()
        {
            base.Awake();

            userInput = FindObjectOfType<UserInput>();
            group = GetComponent<CanvasGroup>();
            playerController = FindObjectOfType<PlayerController>();
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
            timeline = FightTimeline.Instance;
            spotSelector = FindObjectOfType<SpotSelector>();
            roleSelector = FindObjectOfType<RoleSelector>();
            if (playerController != null && playerTargeting == null)
                playerTargeting = playerController.gameObject.GetComponent<TargetController>();

            if (playerTargeting != null)
                playerTargeting.SetConfigMenu(this);

            menuVisible = false;
            CloseWindow();
            applyPopupVisible = false;
            applyPopup.CloseWindow();

            if (thirdPersonCamera != null && thirdPersonCamera.freecam != null)
            {
                originalMouseSensitivity = thirdPersonCamera.mouseSensitivity;
                originalControllerSensitivity = thirdPersonCamera.controllerSensitivity;
                originalRotationSpeed = thirdPersonCamera.freecam.rotationSpeed;
            }
        }

        private void Update()
        {
            if (timeline == null)
                timeline = FightTimeline.Instance;

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
#if !UNITY_WEBPLAYER
                hardwareCursor = toggleMouseCursorType.toggles[0].isOn;
                newHardwareCursor = hardwareCursor;
                if (CursorHandler.Instance != null)
                    CursorHandler.Instance.useSoftwareCursor = !hardwareCursor;
#endif
            }
        }

        public void ToggleConfigMenu()
        {
            menuVisible = !menuVisible;

            if (menuVisible)
            {
                OpenWindow();
            }
            else
            {
                CloseWindow();
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
                applyPopup.Group.alpha = 1f;
                applyPopup.Group.blocksRaycasts = true;
                applyPopup.Group.interactable = true;
                applyPopup.OpenWindow();
            }
            else
            {
                applyPopup.Group.alpha = 0f;
                applyPopup.Group.blocksRaycasts = false;
                applyPopup.Group.interactable = false;
                applyPopup.CloseWindow();
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

        public void ChangeCameraSensitivity(string value)
        {
            if (float.TryParse(value, out float result))
            {
                ChangeCameraSensitivity(result);
            }
        }

        public void ChangeCameraSensitivity(int value)
        {
            ChangeCameraSensitivity((float)value);
        }

        public void ChangeCameraSensitivity(float value)
        {
            configSaved = false;
            newCameraSensitivity = value;
        }

        public void ChangeMouseCursorType(bool value)
        {
#if UNITY_WEBPLAYER
            return;
#else
            configSaved = false;
            newHardwareCursor = value;
#endif
        }

        public void ChangeSkipUpdates(bool value)
        {
            configSaved = false;
            newSkipUpdates = value;
        }

        public void ChangeBotNameType(int value)
        {
            configSaved = false;
            newBotNameType = value;
        }

        public void ChangeBotNameRoleColors(bool value)
        {
            configSaved = false;
            newBotNameRoleColors = value;
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
#if !UNITY_WEBPLAYER
            fullscreen = newFullscreen;
            Screen.fullScreen = fullscreen;
            if (Screen.fullScreen)
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            else
                Screen.fullScreenMode = FullScreenMode.Windowed;
            resolution = newResolution;
            resolutionDropdown.SetResolution(resolution, Screen.fullScreen);
#endif
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
            cameraSensitivity = newCameraSensitivity;
            if (thirdPersonCamera != null)
            {
                float finalSensitivity = Utilities.Map(cameraSensitivity, 1, 30, 0.1f, 3f);
                thirdPersonCamera.mouseSensitivity = originalMouseSensitivity * finalSensitivity;
                thirdPersonCamera.controllerSensitivity = originalControllerSensitivity * finalSensitivity;
                thirdPersonCamera.freecam.rotationSpeed = originalRotationSpeed * finalSensitivity;
            }
#if !UNITY_WEBPLAYER
            hardwareCursor = newHardwareCursor;
            if (CursorHandler.Instance != null)
                CursorHandler.Instance.useSoftwareCursor = !hardwareCursor;
            skipUpdates = newSkipUpdates;
#endif
            botNameType = newBotNameType;
            botNameRoleColors = newBotNameRoleColors;
            if (timeline != null)
            {
                timeline.botNameType = botNameType;
                timeline.colorBotNamesByRole = botNameRoleColors;
                if (spotSelector != null)
                    spotSelector.Select();
                if (roleSelector != null)
                    roleSelector.Select();
            }
            configSaved = true;
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

        public void ForceCloseSettings()
        {
            menuVisible = true;

            if (!configSaved)
                CancelSettings();

            ToggleApplyPopup(false);
            ToggleConfigMenu();
        }

        public void DefaultSettings()
        {
            newScale = 50f;
            scaleSliderSync.Slider.value = newScale;
            scaleSliderSync.Sync(0);
            newLegacy = true;
            toggleMovement.toggles[0].SetIsOnWithoutNotify(true);
            toggleMovement.toggles[1].SetIsOnWithoutNotify(false);
#if !UNITY_WEBPLAYER
            newFullscreen = true;
            newResolution = 0;
#endif
            newVolume = 100f;
            volumeSliderSync.Slider.value = newVolume;
            volumeSliderSync.Sync(0);
            newInvertCameraVertical = false;
            newInvertCameraHorizontal = false;
            newCameraSensitivity = 10f;
            cameraSensitivitySync.Slider.value = newCameraSensitivity;
            cameraSensitivitySync.Sync(0);
            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.invertY = newInvertCameraVertical;
                thirdPersonCamera.invertX = newInvertCameraHorizontal;
            }
            newCameraAdjustment = 0;
            if (playerController != null)
                playerController.autoAdjustCamera = CameraAdjustMode.Moving;
            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.mouseSensitivity = originalMouseSensitivity;
                thirdPersonCamera.controllerSensitivity = originalControllerSensitivity;
                thirdPersonCamera.freecam.rotationSpeed = originalRotationSpeed;
            }
#if !UNITY_WEBPLAYER
            newHardwareCursor = true;
            newSkipUpdates = false;
#endif
            newBotNameType = 0;
            newBotNameRoleColors = false;
            ApplySettings();
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
#if !UNITY_WEBPLAYER
            newFullscreen = fullscreen;
            fullscreenToggle.SetIsOnWithoutNotify(newFullscreen);
            newResolution = resolution;
            resolutionDropdown.SetResolutionWithoutNotify(newResolution, newFullscreen);
#endif
            newVolume = volume;
            volumeSliderSync.Slider.value = newVolume;
            volumeSliderSync.Sync(0);
            newInvertCameraVertical = invertCameraVertical;
            newInvertCameraHorizontal = invertCameraHorizontal;
            invertVerticalCameraToggle.SetIsOnWithoutNotify(newInvertCameraVertical);
            invertHorizontalCameraToggle.SetIsOnWithoutNotify(newInvertCameraHorizontal);
            newCameraAdjustment = cameraAdjustment;
            cameraAdjustmentDropdown.SetValueWithoutNotify(newCameraAdjustment);
            newCameraSensitivity = cameraSensitivity;
            cameraSensitivitySync.Slider.value = newCameraSensitivity;
            cameraSensitivitySync.Sync(0);
#if !UNITY_WEBPLAYER
            newHardwareCursor = hardwareCursor;
            toggleMouseCursorType.toggles[0].SetIsOnWithoutNotify(newHardwareCursor);
            toggleMouseCursorType.toggles[1].SetIsOnWithoutNotify(!newHardwareCursor);
            newSkipUpdates = skipUpdates;
            skipUpdatesToggle.SetIsOnWithoutNotify(newSkipUpdates);
#endif
            newBotNameType = botNameType;
            newBotNameRoleColors = botNameRoleColors;
            ApplySettings();
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
}