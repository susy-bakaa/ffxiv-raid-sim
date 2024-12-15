using System.Collections;
using System.Collections.Generic;
using susy_baka.raidsim.UserInterface;
using UnityEngine;
using UnityEngine.Events;

public class HudEditor : MonoBehaviour
{
    CanvasGroup group;

    public PauseMenu pauseMenu;
    public bool isEditorOpen;
    public bool isMenuOpen;
    public List<ToggleCanvasGroup> hudWidgets = new List<ToggleCanvasGroup>();
    private List<DraggableWindowScript> hudWindows = new List<DraggableWindowScript>();

    public UnityEvent onEditorOpened;
    public UnityEvent onEditorClosed;
    public UnityEvent onMenuOpened;
    public UnityEvent onMenuClosed;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenu>();
        for (int i = 0; i < hudWidgets.Count; i++)
        {
            DraggableWindowScript dws = hudWidgets[i].GetComponentInChildren<DraggableWindowScript>();

            if (dws != null)
            {
                hudWindows.Add(dws);
            }
        }
    }

    /*void Start()
    {
        ToggleHudEditor(false);
        CloseHudEditorMenu(true);
    }*/

    public void ToggleHudEditor()
    {
        ToggleHudEditor(!isEditorOpen);
    }

    public void ToggleHudEditor(bool state)
    {
        isEditorOpen = state;
        for (int i = 0; i < hudWidgets.Count; i++)
        {
            hudWidgets[i].ToggleAlpha(isEditorOpen);
        }
        if (isEditorOpen)
            onEditorOpened.Invoke();
        else
            onEditorClosed.Invoke();
        if (pauseMenu != null)
            pauseMenu.ClosePauseMenu();
    }

    public void ToggleHudEditorMenu()
    {
        if (!isEditorOpen)
            return;

        if (isMenuOpen)
            CloseHudEditorMenu();
        else
            OpenHudEditorMenu();
    }

    public void CloseHudEditorMenu()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        isMenuOpen = false;
        onMenuClosed.Invoke();
        if (pauseMenu != null)
            pauseMenu.ClosePauseMenu();
    }

    public void OpenHudEditorMenu()
    {
        if (!isEditorOpen)
            return;

        group.alpha = 1f;
        group.blocksRaycasts = true;
        isMenuOpen = true;
        onMenuOpened.Invoke();
        if (pauseMenu != null)
            pauseMenu.ClosePauseMenu();
    }

    public void ResetHudLayout()
    {
        for (int i = 0; i < hudWindows.Count; i++)
        {
            hudWindows[i].ResetPosition();
        }
    }
}
