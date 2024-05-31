using System;
using System.Collections;
using System.Collections.Generic;
using no00ob.WaveSurvivalGame.UserInterface;
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour
{
    CanvasGroup group;
    PlayerController playerController;

    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private SliderInputLinker scaleSliderSync;
    [SerializeField] private ToggleLinker toggleMovement;
    float scale;
    float newScale;
    bool legacy;
    bool newLegacy;
    [SerializeField] private CanvasGroup applyPopup;

    bool menuVisible = false;
    bool applyPopupVisible = false;
    bool configSaved = true;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        playerController = FindObjectOfType<PlayerController>();

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
        }
        else
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
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
        }
        else
        {
            applyPopup.alpha = 0f;
            applyPopup.blocksRaycasts = false;
            applyPopup.interactable = false;
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
        playerController.legacyMovement = newLegacy;
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

    public void DefaultSettings()
    {
        newScale = 50f;
        scaleSliderSync.Slider.value = newScale;
        scaleSliderSync.Sync(0);
        newLegacy = true;
        toggleMovement.toggles[0].SetIsOnWithoutNotify(true);
        toggleMovement.toggles[1].SetIsOnWithoutNotify(false);
        ApplySettings();
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
        ApplySettings();
    }
}
