// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public enum BindType { none, action, macro }

        private HotbarController controller;
        private Image image;
        private Button button;
        private CanvasGroup group;
        private HudElement element;

        [Header("Info")]
        [SerializeField] private CharacterAction action;
        //private Macro macro;
        [SerializeField] private CharacterActionData lastAction;
        private int slotIndex;
        private SlotBinding binding;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup borderStandard;
        [SerializeField] private CanvasGroup borderDark;
        [SerializeField] private Animator recastFillAnimator;
        [SerializeField] private CanvasGroup recastFillGroup;
        [SerializeField] private CanvasGroup selectionBorder;
        [SerializeField] private CanvasGroup clickHighlight;
        [SerializeField] private CanvasGroup comboOutlineGroup;
        [SerializeField] private TextMeshProUGUI recastTimeText;
        [SerializeField] private TextMeshProUGUI resourceCostText;
        [SerializeField] private TextMeshProUGUI keybindText;
        [SerializeField] private HudElementColor[] hudElements;
        [SerializeField] private List<Color> defaultColors;
        [SerializeField] private List<Color> unavailableColors;

        private string[] animations = new string[]
        {
        "ui_hotbar_recast_type1_none",
        "ui_hotbar_recast_type1_fill",
        "ui_hotbar_recast_type2_fill",
        "ui_hotbar_recast_type3_fill"
        };
        private int[] animationHashes = new int[0];
        private bool pointer;
        private bool colorsFlag;
        private bool animFlag;

        private void Awake()
        {
            image = GetComponent<Image>();
            button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
            element = GetComponent<HudElement>();

            animationHashes = new int[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                animationHashes[i] = Animator.StringToHash(animations[i]);
            }
        }

        private void Update()
        {
            if (controller == null)
                return;
            UpdateRuntimeVisuals();
        }

        public void Bind(HotbarController controller, int slotIndex, SlotBinding binding)
        {
            this.controller = controller;
            this.slotIndex = slotIndex;
            this.binding = binding;

            switch (binding.kind)
            {
                case SlotKind.Empty:
                    action = null;
                    //macro = null;
                    break;
                case SlotKind.Action:
                    action = controller.Registry.GetById(binding.id);
                    //macro = null;
                    break;
                case SlotKind.Macro:
                    //action = null;
                    //macro = controller.MacroLibrary.Get(binding.id);
                    break;
            }

            if (button != null)
            {
                //Debug.Log($"action {gameObject.name} of {controller.gameObject.name} button was linked!");
                //button.onClick.AddListener(() => { controller.PerformAction(action); });
            }

            // Set visuals once (icon/name)
            ApplyStaticVisuals();
        }

        private void ApplyStaticVisuals()
        {
            // Macro: show macro icon/name
            // Action: pull icon/name from CharacterActionData
            // Keep this lightweight and cached if you want.

            image.sprite = action.Data.icon;

            if (borderStandard != null)
            {
                borderStandard.alpha = 1f;
            }
            if (borderDark != null)
            {
                borderDark.alpha = 0f;
            }
            if (selectionBorder != null)
            {
                selectionBorder.alpha = 0f;
            }
            if (clickHighlight != null)
            {
                clickHighlight.alpha = 0f;
            }
            if (recastFillGroup != null)
            {
                recastFillGroup.alpha = 0f;
            }
            if (recastFillAnimator != null)
            {
                recastFillAnimator.speed = 0f;
                recastFillAnimator.Play("ui_hotbar_recast_type1_none");
            }
            if (resourceCostText != null)
            {
                if (action.Data.manaCost > 0)
                {
                    resourceCostText.text = action.Data.manaCost.ToString();
                }
                else
                {
                    resourceCostText.text = string.Empty;
                }
            }
            if (comboOutlineGroup != null)
            {
                comboOutlineGroup.alpha = 0f;
            }

            for (int i = 0; i < hudElements.Length; i++)
            {
                hudElements[i].SetColor(defaultColors);
            }
        }

        private void UpdateRuntimeVisuals()
        {
            // Default:
            //disabledOverlay?.SetActive(false);
            //cooldownFill.fillAmount = 0f;

            if (binding.kind != SlotKind.Action)
                return;

            // Resolve action runtime state
            // You need SOME way to query cooldown/usable. Best is to add a tiny interface on CharacterAction.
            //var action = /* resolve via registry somehow or cache during ApplyStaticVisuals */;
            if (!action)
                return;

            // Example shape (adapt to your system):
            // float cdRemaining = action.CooldownRemaining;
            // float cdTotal = action.CooldownDuration;
            // bool can = action.CanExecute(out var reason);

            // cooldownFill.fillAmount = (cdTotal > 0f) ? Mathf.Clamp01(cdRemaining / cdTotal) : 0f;
            // disabledOverlay.SetActive(!can);

            if (action.unavailable)
            {
                if (!colorsFlag)
                {
                    for (int i = 0; i < hudElements.Length; i++)
                    {
                        hudElements[i].SetColor(unavailableColors);
                    }
                    colorsFlag = true;
                }

                if (resourceCostText != null)
                {
                    resourceCostText.text = "X";
                }
                
                //chargesLeft = action.Data.charges;
            }

            if (group != null)
            {
                if (action.invisible)
                {
                    group.alpha = 0f;
                    group.interactable = false;
                    group.blocksRaycasts = false;
                }
                else
                {
                    group.alpha = 1f;
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }
            }

            if (action.Data.range > 0f && action.Data.isTargeted && (action.distanceToTarget > action.Data.range) && action.hasTarget && !action.unavailable)
            {
                if (!colorsFlag)
                {
                    for (int i = 0; i < hudElements.Length; i++)
                    {
                        hudElements[i].SetColor(unavailableColors);
                    }
                    colorsFlag = true;
                }

                if (resourceCostText != null)
                {
                    if (action.Data.manaCost <= 0 && action.Data.charges < 2)
                    {
                        resourceCostText.text = "X";
                    }
                    else if (action.Data.manaCost > 0)
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                    else if (action.Data.charges > 1)
                    {
                        resourceCostText.text = action.chargesLeft.ToString();
                    }
                }
            }
            else if (!action.unavailable)
            {
                if (colorsFlag)
                {
                    for (int i = 0; i < hudElements.Length; i++)
                    {
                        hudElements[i].SetColor(defaultColors);
                    }
                    colorsFlag = false;
                }

                if (resourceCostText != null)
                {
                    if (action.Data.manaCost <= 0 && action.Data.charges < 2)
                    {
                        resourceCostText.text = string.Empty;
                    }
                    else if (action.Data.manaCost > 0)
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                    else if (action.Data.charges > 1)
                    {
                        resourceCostText.text = action.chargesLeft.ToString();
                    }
                }
            }
            
            if (action.RecastTimer > 0f)
            {
                if (recastFillGroup != null)
                {
                    recastFillGroup.alpha = 1f;
                }
                if (recastTimeText != null && action.LastRecastTimer > 2.5f)
                {
                    recastTimeText.text = action.RecastTimer.ToString("F0");
                }
            }
            else
            {
                if (recastFillGroup != null)
                {
                    recastFillGroup.alpha = 0f;
                }
                if (recastTimeText != null)
                {
                    recastTimeText.text = "";
                }
                if (borderStandard != null)
                {
                    borderStandard.alpha = 1f;
                }
                if (action.RecastType == RecastType.longGcd && borderDark != null)
                {
                    borderDark.alpha = 0f;
                }
            }

            if (action.showOutline)
            {
                comboOutlineGroup.alpha = 1f;
            }
            else if (comboOutlineGroup != null)
            {
                comboOutlineGroup.alpha = 0f;
            }

            if (borderStandard != null && action.RecastTimer > 0f)
            {
                if (action.RecastType == RecastType.stackedOgcd)
                {
                    borderStandard.alpha = 1f;
                }
                else
                {
                    borderStandard.alpha = 0f;
                }

                if (action.RecastType == RecastType.longGcd && borderDark != null)
                {
                    borderDark.alpha = 1f;
                }
            }
            if (recastFillAnimator != null && action.RecastTimer > 0f)
            {
                animFlag = false;
                recastFillAnimator.Play(animationHashes[(int)action.RecastType + 1], 0, Utilities.Map(action.LastRecastTimer - action.RecastTimer, 0f, action.LastRecastTimer, 0f, 1f));
            }
            else if (recastFillAnimator != null && !animFlag)
            {
                animFlag = true;
                recastFillAnimator.Play(animationHashes[0]);
            }

            if (button != null)
            {
                if (!action.isDisabled && !action.unavailable)
                {
                    button.interactable = action.isAvailable;
                }
                else
                {
                    button.interactable = false;
                }
            }

            if (keybindText != null)
            {
                //if (currentKeybind != null)
                //{
                //    keybindText.text = currentKeybind.ToShortString();
                //}
                //else
                //{
                    keybindText.text = string.Empty;
                //}
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (action.unavailable)
                return;

            pointer = true;
            if (selectionBorder != null)
                selectionBorder.LeanAlpha(1f, 0.25f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (action.unavailable)
                return;

            pointer = false;
            if (selectionBorder != null)
                selectionBorder.LeanAlpha(0f, 0.25f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (action.unavailable)
                return;

            if (clickHighlight == null) //(eventData == null && pointer) || 
            {
                return;
            }

            if (element != null)
            {
                element.onPointerClick.Invoke(new HudElementEventInfo(element, eventData));
            }

            clickHighlight.transform.localScale = new Vector3(0f, 0f, 1f);
            clickHighlight.transform.LeanScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);//.setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.15f); });
            Utilities.FunctionTimer.Create(this, () => clickHighlight.LeanAlpha(0f, 0.15f), 0.3f, $"{action.ActionId}_ui_click_animation_fade_delay", true, false);
            clickHighlight.LeanAlpha(1f, 0.15f);
            if (!pointer)
                selectionBorder.LeanAlpha(1f, 0.15f).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => selectionBorder.LeanAlpha(0f, 0.15f), 0.2f, $"{action.ActionId}_ui_click_highlight_fade_delay", true, false));
        }
    }
}