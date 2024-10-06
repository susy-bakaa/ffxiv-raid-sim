using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCanvasGroup : MonoBehaviour
{
    [SerializeField] private bool startOff = true;

    private CanvasGroup group;
    private bool state = false;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (startOff)
        {
            ToggleAlpha(false);
        }
    }

    public void ToggleAlpha()
    {
        state = !state;
        ToggleAlpha(state);
    }

    public void ToggleAlpha(bool state)
    {
        this.state = state;

        if (this.state)
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
}
