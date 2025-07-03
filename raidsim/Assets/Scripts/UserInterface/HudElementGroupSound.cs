using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using dev.susybaka.Shared.Audio;
using Unity.VisualScripting;

[RequireComponent(typeof(HudElementGroup))]
public class HudElementGroupSound : MonoBehaviour
{
    AudioManager audioManager;
    HudElementGroup hudElementGroup;

    public string hoverSound = "ui_hover";
    public string confirmSound = "ui_confirm";
    public string cancelSound = "ui_cancel";

    public bool limitEvents = false;
    public float eventCooldown = 1f;

    private float timer = 0f;
    private bool eventsAvailable = true;
    private bool countdown = false;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
        hudElementGroup = GetComponent<HudElementGroup>();

        hudElementGroup.onPointerEnter.AddListener(OnPointerEnter);
        hudElementGroup.onPointerExit.AddListener(OnPointerExit);
        hudElementGroup.onClick.AddListener(OnClick);

        timer = eventCooldown;
    }

    private void Update()
    {
        if (limitEvents && countdown)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                countdown = false;
                eventsAvailable = true;
            }
        }
    }

    private void OnPointerEnter(PointerEventData data)
    {
        if (limitEvents)
        {
            countdown = false;
            timer = eventCooldown;
        }

        if (!eventsAvailable)
            return;

        audioManager.Play(hoverSound);

        if (limitEvents)
            eventsAvailable = false;
    }

    private void OnPointerExit(PointerEventData data)
    {
        if (limitEvents)
        {
            countdown = true;
        }
    }

    private void OnClick(Button button)
    {
        audioManager.Play(confirmSound);
    }
}
