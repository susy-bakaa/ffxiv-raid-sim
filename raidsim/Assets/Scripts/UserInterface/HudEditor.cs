using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.raidsim.SaveLoad;

namespace dev.susybaka.raidsim.UI
{
    public class HudEditor : MonoBehaviour
    {
        CanvasGroup group;
        UserInput userInput;

        public PauseMenu pauseMenu;
        public bool isEditorOpen;
        public bool isMenuOpen;
        public HudPreset[] presets;
        public HudPreset currentPreset;
        public List<HudWidget> hudWidgets = new List<HudWidget>();

        public UnityEvent onEditorOpened;
        public UnityEvent onEditorClosed;
        public UnityEvent onMenuOpened;
        public UnityEvent onMenuClosed;
        public UnityEvent<int> onHudPresetChanged;

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (hudWidgets.Count > 0)
            {
                for (int i = 0; i < hudWidgets.Count; i++)
                {
                    if (string.IsNullOrEmpty(hudWidgets[i].name) && hudWidgets[i].group != null)
                    {
                        HudWidget widget = hudWidgets[i];
                        widget.name = hudWidgets[i].group.gameObject.name;
                        widget.name = widget.name.Replace("Widget", "");
                        hudWidgets[i] = widget;
                    }
                }
            }
        }
#endif

        private void Awake()
        {
            userInput = FindObjectOfType<UserInput>();
            group = GetComponent<CanvasGroup>();
            if (pauseMenu == null)
                pauseMenu = FindObjectOfType<PauseMenu>();
        }

        private void Update()
        {
            if (isEditorOpen && userInput != null)
            {
                userInput.inputEnabled = false;
                userInput.zoomInputEnabled = false;
                userInput.movementInputEnabled = false;
                userInput.rotationInputEnabled = false;
                userInput.targetRaycastInputEnabled = false;
            }
        }

        public void ToggleHudEditor()
        {
            ToggleHudEditor(!isEditorOpen);
        }

        public void ToggleHudEditor(bool state)
        {
            isEditorOpen = state;
            for (int i = 0; i < hudWidgets.Count; i++)
            {
                if (hudWidgets[i].group != null)
                    hudWidgets[i].group.ToggleAlpha(isEditorOpen);
            }
            if (isEditorOpen)
            {
                onEditorOpened.Invoke();
                if (userInput != null)
                {
                    userInput.inputEnabled = false;
                    userInput.zoomInputEnabled = false;
                    userInput.movementInputEnabled = false;
                    userInput.rotationInputEnabled = false;
                    userInput.targetRaycastInputEnabled = false;
                }
            }
            else
            {
                onEditorClosed.Invoke();
                if (userInput != null)
                {
                    userInput.inputEnabled = true;
                    userInput.zoomInputEnabled = true;
                    userInput.movementInputEnabled = true;
                    userInput.rotationInputEnabled = true;
                    userInput.targetRaycastInputEnabled = true;
                }
            }
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
            if (userInput != null)
            {
                userInput.inputEnabled = true;
                userInput.zoomInputEnabled = true;
                userInput.movementInputEnabled = true;
                userInput.rotationInputEnabled = true;
                userInput.targetRaycastInputEnabled = true;
            }
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
            if (userInput != null)
            {
                userInput.inputEnabled = false;
                userInput.zoomInputEnabled = false;
                userInput.movementInputEnabled = false;
                userInput.rotationInputEnabled = false;
                userInput.targetRaycastInputEnabled = false;
            }
        }

        public void ResetHudLayout()
        {
            for (int i = 0; i < hudWidgets.Count; i++)
            {
                if (hudWidgets[i].window != null)
                    hudWidgets[i].window.ResetPosition();
            }
        }

        public void SelectHudPreset(int index)
        {
            if (index < 0 || index >= presets.Length)
                return;

            currentPreset = presets[index];

            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i].toggle != null)
                    presets[i].toggle.Toggle(i == index);
                if (presets[i].button != null)
                    presets[i].button.interactable = i != index;
            }

            UpdateHudWidgets();

            onHudPresetChanged.Invoke(index);
        }

        private void UpdateHudWidgets()
        {
            for (int i = 0; i < hudWidgets.Count; i++)
            {
                if (hudWidgets[i].save == null)
                    continue;

                hudWidgets[i].save.id = currentPreset.index.ToString();
                if (hudWidgets[i].window != null && hudWidgets[i].window.targetSave != null)
                {
                    hudWidgets[i].window.targetSave.id = currentPreset.index.ToString();
                    hudWidgets[i].save.Reload();
                    hudWidgets[i].window.targetSave.Reload();
                }
                else
                {
                    hudWidgets[i].save.Reload();
                }
            }
        }

        [System.Serializable]
        public struct HudPreset
        {
            public string name;
            public int index;
            public ToggleImage toggle;
            public Button button;
        }

        [System.Serializable]
        public struct HudWidget
        {
            public string name;
            public ToggleCanvasGroup group;
            public DraggableWindowScript window;
            public SavePosition save;
        }
    }
}