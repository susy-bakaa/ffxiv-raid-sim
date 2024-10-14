using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    CanvasGroup group;

    public HudEditor hudEditor;
    public TargetController playerTargeting;
    public string menuScene = "menu";
    public bool isOpen = false;
    //public bool pauseOnOpen = false;
    public UnityEvent onPaused;
    public UnityEvent onUnpaused;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (playerTargeting == null)
            playerTargeting = GameObject.Find("Player").GetComponent<TargetController>();
        if (hudEditor == null)
            hudEditor = FindObjectOfType<HudEditor>();
        playerTargeting.SetPauseMenu(this);
    }

    /*void Start()
    {
        ClosePauseMenu(true);
    }*/

    /*void Update()
    {
        if (pauseOnOpen && isOpen)
        {
            FightTimeline.Instance.paused = true;
        } 
        else if (pauseOnOpen && !isOpen)
        {
            FightTimeline.Instance.paused = false;
        }
    }*/

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void TogglePauseMenu()
    {
        if (isOpen)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    public void ClosePauseMenu()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        isOpen = false;
        FightTimeline.Instance.TogglePause(false, "pause_menu");
        onUnpaused.Invoke();
    }

    public void OpenPauseMenu()
    {
        if (playerTargeting.currentTarget != null)
            return;
        if (hudEditor != null && hudEditor.isEditorOpen)
            return;
        if (hudEditor != null && hudEditor.isMenuOpen)
            return;

        group.alpha = 1f;
        group.blocksRaycasts = true;
        isOpen = true;
        FightTimeline.Instance.TogglePause(true, "pause_menu");
        onPaused.Invoke();
    }

    public void ClickButton(Button button)
    {
        button.onClick.Invoke();
    }
}
