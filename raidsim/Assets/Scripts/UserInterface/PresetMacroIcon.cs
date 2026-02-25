// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.UI.MacroIconData;

namespace dev.susybaka.raidsim.UI
{
    public class PresetMacroIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private MacroLibrary library;
        private MacroEditor editor;
        private Image image;
        private HudElement element;

        [Header("Info")]
        [SerializeField] private Entry iconData;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup borderStandard;
        [SerializeField] private CanvasGroup selectionBorder;
        [SerializeField] private CanvasGroup clickHighlight;
        [SerializeField] private Image selectionIcon;

        private bool pointer;
        private System.Action<int> onClick;
        private int index = 0;

        private void Awake()
        {
            image = GetComponent<Image>();
            element = GetComponent<HudElement>();
        }

        public void Initialize(MacroLibrary library, MacroEditor editor, Entry icon, int index, System.Action<int> callback)
        {
            this.library = library;
            this.editor = editor;
            this.iconData = icon;
            this.index = index;
            onClick = callback;

            // Set visuals once (icon/name)
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            image.sprite = iconData.sprite;

            if (borderStandard != null)
            {
                borderStandard.alpha = 1f;
            }
            if (selectionBorder != null)
            {
                selectionBorder.alpha = 0f;
            }
            if (clickHighlight != null)
            {
                clickHighlight.alpha = 0f;
            }
            if (selectionIcon != null)
            {
                int i = editor.GetCurrentIndex();
                if (i < 0 || i > MacroLibrary.Count)
                    i = 0;
                string iconId = library.Get(i).customIconId;
                if (!string.IsNullOrEmpty(iconId) && iconId == iconData.id)
                {
                    selectionIcon.gameObject.SetActive(true);
                }
                else
                {
                    selectionIcon.gameObject.SetActive(false);
                }
            }
        }

        public void RefreshVisuals()
        {
            if (selectionIcon != null)
            {
                int i = editor.GetCurrentIndex();
                if (i < 0 || i > MacroLibrary.Count)
                    i = 0;
                string iconId = library.Get(i).customIconId;
                if (!string.IsNullOrEmpty(iconId) && iconId == iconData.id)
                {
                    selectionIcon.gameObject.SetActive(true);
                }
                else
                {
                    selectionIcon.gameObject.SetActive(false);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointer = true;
            if (selectionBorder != null)
                selectionBorder.LeanAlpha(1f, 0.25f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointer = false;
            if (selectionBorder != null)
                selectionBorder.LeanAlpha(0f, 0.25f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(index);

            if (clickHighlight == null)
            {
                return;
            }

            if (element != null)
            {
                element.onPointerClick.Invoke(new HudElementEventInfo(element, eventData));
            }

            clickHighlight.transform.localScale = new Vector3(0f, 0f, 1f);
            clickHighlight.transform.LeanScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);//.setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.15f); });
            Utilities.FunctionTimer.Create(this, () => clickHighlight.LeanAlpha(0f, 0.15f), 0.3f, $"{iconData.id}_ui_click_animation_fade_delay", true, false);
            clickHighlight.LeanAlpha(1f, 0.15f);
            if (!pointer)
                selectionBorder.LeanAlpha(1f, 0.15f).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => selectionBorder.LeanAlpha(0f, 0.15f), 0.2f, $"{iconData.id}_ui_click_highlight_fade_delay", true, false));
        }
    }
}