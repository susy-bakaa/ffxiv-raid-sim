using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HudElement))]
public class PartyCastBarHelper : MonoBehaviour
{
    private HudElement element;
    private CharacterState character;
    private ActionController actionController;

    public CanvasGroup castBarBackground;
    public float fade = 0.2f;

    private bool alphaSet = false;

    private void Awake()
    {
        element = GetComponent<HudElement>();
        character = element.characterState;

        if (character != null)
        {
            actionController = character.GetComponent<ActionController>();
        }

        if (castBarBackground != null)
            castBarBackground.alpha = 0f;
    }

    private void Update()
    {
        if (actionController != null && castBarBackground != null)
        {
            if (actionController.isCasting && !alphaSet)
            {
                castBarBackground.LeanAlpha(1f, fade);
                alphaSet = true;
            }
            else if (!actionController.isCasting && alphaSet)
            {
                castBarBackground.LeanAlpha(0f, fade);
                alphaSet = false;
            }
        }
    }
}
