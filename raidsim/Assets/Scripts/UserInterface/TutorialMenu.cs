using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMenu : MonoBehaviour
{
    private CanvasGroup group;
    [SerializeField] private UserInput input;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;

        if (input == null)
            return;

        input.inputEnabled = false;
        input.movementInputEnabled = false;
        input.zoomInputEnabled = false;
        input.rotationInputEnabled = false;
        input.targetRaycastInputEnabled = false;
    }

    public void Hide()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        if (input == null)
            return;

        input.inputEnabled = true;
        input.movementInputEnabled = true;
        input.zoomInputEnabled = true;
        input.rotationInputEnabled = true;
        input.targetRaycastInputEnabled = true;
    }

    public void Toggle(bool state)
    {
        if (!state)
            Show();
        else
            Hide();
    }
}
