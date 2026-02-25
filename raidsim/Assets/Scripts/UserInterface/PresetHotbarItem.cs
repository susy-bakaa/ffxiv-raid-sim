// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class PresetHotbarItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IEndDragHandler
    {
        private DraggableHotbarPayload payload;
        private CanvasGroup group;
        private Image image;
        private HudElement element;
        private CanvasGroup tooltipGroup;
        private MacroSlot macroSlot;

        [Header("Info")]
        [SerializeField] private HotbarController controller;
        [SerializeField] private MacroEditor macroEditor;
        [SerializeField] private CharacterAction action;
        [SerializeField] private MacroEntry macro;
        [SerializeField] private SlotBinding binding;
        public event System.Action<SlotBinding> OnClick;

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
        [SerializeField] private TextMeshProUGUI chargesText;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private HudElementColor[] hudElements;
        [SerializeField] private List<Color> defaultColors;
        [SerializeField] private List<Color> unavailableColors;

        private readonly string[] animations = new string[]
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
        private bool dragging = false;
        private string tooltip = string.Empty;

        private void Awake()
        {
            payload = GetComponent<DraggableHotbarPayload>();
            image = GetComponent<Image>();
            group = GetComponent<CanvasGroup>();
            element = GetComponent<HudElement>();
            tooltipGroup = tooltipText?.GetComponentInParent<CanvasGroup>();
            tooltipGroup.gameObject.SetActive(false);

            animationHashes = new int[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                animationHashes[i] = Animator.StringToHash(animations[i]);
            }
        }

        private void Update()
        {
            UpdateRuntimeVisuals();
        }

        public void Initialize(HotbarController controller, MacroEditor macroEditor, SlotBinding binding)
        {
            this.controller = controller;
            this.macroEditor = macroEditor;
            this.binding = binding;
            this.tooltip = string.Empty;

            switch (binding.kind)
            {
                case SlotKind.Empty:
                    action = null;
                    macro = new MacroEntry { isValid = false }; // empty
                    if (payload != null)
                        payload.sourceKind = DragSourceKind.PaletteAction;
                    break;
                case SlotKind.Action:
                    // Resolve the action for presentation (icon/name) purposes.
                    action = controller.GetResolvedAction(binding.id, ActionResolveMode.Presentation);
                    macro = new MacroEntry { isValid = false }; // empty
                    tooltip = action.GetFullActionName();
                    if (payload != null)
                        payload.sourceKind = DragSourceKind.PaletteAction;
                    break;
                case SlotKind.Macro:
                    action = null;
                    int i = -1;
                    MacroLibrary.TryParseMacroId(binding.id, out i);
                    macro = macroEditor.Library.Get(i);
                    if (!macro.isValid)
                        macro = new MacroEntry { isValid = false };
                    if (macro.isValid && !string.IsNullOrEmpty(macro.name))
                        tooltip = macro.name;
                    else
                        tooltip = $"Macro #{i}";
                    if (payload != null)
                        payload.sourceKind = DragSourceKind.PaletteMacro;
                    if (macroSlot == null)
                        macroSlot = transform.GetComponentInParents<MacroSlot>();
                    break;
            }

            // Set an optional payload for this item so that it can be moved with drag and drop even after it is placed into a slot.
            if (payload != null)
            {
                payload.binding = binding;
                payload.fromGroupId = string.Empty;
                payload.fromPageIndex = -1;
                payload.fromSlotIndex = -1;
            }

            // Set visuals once (icon/name)
            ApplyStaticVisuals();
        }

        private void ApplyStaticVisuals()
        {
            // Macro: show macro icon/name
            // Action: pull icon/name from CharacterActionData
            // Keep this lightweight and cached if you want.

            if (macro.isValid && macroEditor != null)
            {
                image.sprite = macroEditor.Resolver.ResolveIconSprite(macro, out action);
                image.color = Color.white;
            }
            else if (action != null)
            {
                image.sprite = action.Data.icon;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
                image.color = new Color(1f, 1f, 1f, 0f); // hide the image if there's no valid action or macro assigned
            }

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
            if (resourceCostText != null && action != null)
            {
                if (action.Data.manaCost <= 0 && !string.IsNullOrEmpty(action.overrideResourceText))
                {
                    resourceCostText.text = string.Empty;
                }
                else if (action.Data.manaCost > 0 && !string.IsNullOrEmpty(action.overrideResourceText))
                {
                    resourceCostText.text = action.Data.manaCost.ToString();
                }
                else
                {
                    resourceCostText.text = action.overrideResourceText;
                }
            } 
            else if (resourceCostText != null)
            {
                resourceCostText.text = string.Empty;
            }
            if (chargesText != null)
            {
                if (action != null && action.Data.charges > 1)
                {
                    chargesText.text = action.Data.charges.ToString();
                }
                else
                {
                    chargesText.text = string.Empty;
                }
            }
            if (comboOutlineGroup != null)
            {
                comboOutlineGroup.alpha = 0f;
            }
            if (tooltipText != null)
            {
                tooltipText.text = tooltip;

                if (!string.IsNullOrEmpty(tooltip))
                {
                    tooltipGroup.alpha = 1f;
                }
                else
                {
                    tooltipGroup.alpha = 0f;
                }
            }

            for (int i = 0; i < hudElements.Length; i++)
            {
                hudElements[i].SetColor(defaultColors);
            }
        }

        private void UpdateRuntimeVisuals()
        {
            if (binding.kind == SlotKind.Empty)
                return;

            if (action == null)
            {
                if (recastFillGroup != null)
                {
                    recastFillGroup.alpha = 0f;
                }
                if (recastTimeText != null)
                {
                    recastTimeText.text = string.Empty;
                }
                if (borderStandard != null)
                {
                    borderStandard.alpha = 1f;
                }
                if (borderDark != null)
                {
                    borderDark.alpha = 0f;
                }
                if (comboOutlineGroup != null)
                {
                    comboOutlineGroup.alpha = 0f;
                }
                if (resourceCostText != null)
                {
                    resourceCostText.text = string.Empty;
                }
                if (chargesText != null)
                {
                    chargesText.text = string.Empty;
                }
                if (colorsFlag)
                {
                    for (int i = 0; i < hudElements.Length; i++)
                    {
                        hudElements[i].SetColor(defaultColors);
                    }
                    colorsFlag = false;
                }
                return;
            }

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
                    resourceCostText.text = "<b>X</b>";
                }
            }

            if (group != null)
            {
                if (dragging)
                {
                    group.blocksRaycasts = false;

                    if (action.invisible)
                    {
                        group.alpha = 0f;
                        group.interactable = false;
                    }
                    else
                    {
                        group.alpha = 1f;
                        group.interactable = true;
                    }
                }
                else
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
                    if (action.Data.manaCost <= 0)
                    {
                        resourceCostText.text = "<b>X</b>";
                    }
                    else if (action.Data.manaCost > 0)
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                }
                if (chargesText != null)
                {
                    if (action.Data.charges > 1)
                    {
                        chargesText.text = action.chargesLeft.ToString();
                    }
                    else
                    {
                        chargesText.text = string.Empty;
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
                    if (action.Data.manaCost <= 0)
                    {
                        resourceCostText.text = string.Empty;
                    }
                    else if (action.Data.manaCost > 0)
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                }
                if (chargesText != null)
                {
                    if (action.Data.charges > 1)
                    {
                        chargesText.text = action.chargesLeft.ToString();
                    }
                    else
                    {
                        chargesText.text = string.Empty;
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
        }

        public void RefreshStaticVisuals()
        {
            switch (binding.kind)
            {
                case SlotKind.Empty:
                    action = null;
                    macro = new MacroEntry { isValid = false }; // empty
                    break;
                case SlotKind.Action:
                    // Resolve the action for presentation (icon/name) purposes.
                    action = controller.GetResolvedAction(binding.id, ActionResolveMode.Presentation);
                    macro = new MacroEntry { isValid = false }; // empty
                    tooltip = action.GetFullActionName();
                    break;
                case SlotKind.Macro:
                    action = null;
                    int i = -1;
                    MacroLibrary.TryParseMacroId(binding.id, out i);
                    macro = macroEditor.Library.Get(i);
                    if (!macro.isValid)
                        macro = new MacroEntry { isValid = false };
                    if (macro.isValid && !string.IsNullOrEmpty(macro.name))
                        tooltip = macro.name;
                    else
                        tooltip = $"Macro #{i}";
                    break;
            }

            if (macro.isValid && macroEditor != null)
            {
                image.sprite = macroEditor.Resolver.ResolveIconSprite(macro, out action);
                image.color = Color.white;
            }
            else if (action != null)
            {
                image.sprite = action.Data.icon;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
                image.color = new Color(1f, 1f, 1f, 0f); // hide the image if there's no valid action or macro assigned
            }

            if (resourceCostText != null && action != null)
            {
                if (action.Data.manaCost <= 0 && !string.IsNullOrEmpty(action.overrideResourceText))
                {
                    resourceCostText.text = string.Empty;
                }
                else if (action.Data.manaCost > 0 && !string.IsNullOrEmpty(action.overrideResourceText))
                {
                    resourceCostText.text = action.Data.manaCost.ToString();
                }
                else
                {
                    resourceCostText.text = action.overrideResourceText;
                }
            }
            else if (resourceCostText != null)
            {
                resourceCostText.text = string.Empty;
            }
            if (chargesText != null)
            {
                if (action != null && action.Data.charges > 1)
                {
                    chargesText.text = action.chargesLeft.ToString();
                }
                else
                {
                    chargesText.text = string.Empty;
                }
            }
            if (tooltipText != null)
            {
                tooltipText.text = tooltip;

                if (!string.IsNullOrEmpty(tooltip))
                {
                    tooltipGroup.alpha = 1f;
                }
                else
                {
                    tooltipGroup.alpha = 0f;
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
            // If we're dragging, we don't want to trigger click events on the item itself since it's being dropped onto something else.
            if (dragging)
                return;

            // First trigger any macro slot click behavior if applicable (e.g. for editing macros).
            if (macroSlot != null)
                macroSlot.OnPointerClick(eventData);

            if (action != null && !macro.isValid)
            {
                if (!action.unavailable)
                    OnClick.Invoke(binding);
            }

            if (clickHighlight == null) //(eventData == null && pointer) || 
            {
                return;
            }

            if (element != null)
            {
                element.onPointerClick.Invoke(new HudElementEventInfo(element, eventData));
            }

            string id = (action != null && !macro.isValid) ? action.ActionId : macro.isValid ? macro.name : gameObject.name;

            clickHighlight.transform.localScale = new Vector3(0f, 0f, 1f);
            clickHighlight.transform.LeanScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);//.setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.15f); });
            Utilities.FunctionTimer.Create(this, () => clickHighlight.LeanAlpha(0f, 0.15f), 0.3f, $"{id}_ui_click_animation_fade_delay", true, false);
            clickHighlight.LeanAlpha(1f, 0.15f);
            if (!pointer)
                selectionBorder.LeanAlpha(1f, 0.15f).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => selectionBorder.LeanAlpha(0f, 0.15f), 0.2f, $"{id}_ui_click_highlight_fade_delay", true, false));
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }

        public void Copy(PresetHotbarItem copy)
        {
            if (copy != null)
            {
                controller = copy.controller;
                macroEditor = copy.macroEditor;
                OnClick = copy.OnClick;
                action = copy.action;
                macro = copy.macro;
                binding = copy.binding;
                tooltip = copy.tooltip;
            }
            else
            {
                controller = null;
                macroEditor = null;
                OnClick = null;
                action = null;
                macro = new MacroEntry { isValid = false };
                binding = new SlotBinding { kind = SlotKind.Empty, id = string.Empty };
                tooltip = string.Empty;
            }
            ApplyStaticVisuals();
        }
    }
}