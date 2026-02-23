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
    public class HotbarItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public enum BindType { none, action, macro }

        private HotbarController controller;
        private HotbarSlot slot;
        private DraggableHotbarPayload payload;
        private CanvasGroup group;
        private Image image;
        private HudElement element;
        private CanvasGroup tooltipGroup;

        [Header("Info")]
        [SerializeField] private CharacterAction action;
        //private Macro macro;
        [SerializeField] private CharacterActionData lastAction;
        private string groupId;
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
            tooltipGroup = tooltipText?.transform.GetComponentInParent<CanvasGroup>();
            tooltipGroup.gameObject.SetActive(false);

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

        public void Bind(HotbarController controller, HotbarSlot slot, SlotBinding binding)
        {
            this.controller = controller;
            this.binding = binding;
            this.slot = slot;
            this.slotIndex = this.slot.SlotIndex;
            this.groupId = this.slot.GroupId;
            this.tooltip = string.Empty;

            switch (binding.kind)
            {
                case SlotKind.Empty:
                    action = null;
                    //macro = null;
                    break;
                case SlotKind.Action:
                    // Resolve the action for presentation (icon/name) purposes.
                    action = controller.GetResolvedAction(binding.id, ActionResolveMode.Presentation);

                    //macro = null;
                    tooltip = action.GetFullActionName();
                    break;
                case SlotKind.Macro:
                    //action = null;
                    //macro = controller.MacroLibrary.Get(binding.id);
                    //tooltip = macro.name;
                    break;
            }

            // Set an optional payload for this item so that it can be moved with drag and drop even after it is placed into a slot.
            if (payload != null)
            {
                payload.controller = controller;
                payload.sourceKind = DragSourceKind.HotbarSlot;
                payload.binding = binding;
                payload.fromGroupId = groupId;
                payload.fromPageIndex = controller.GetActivePage(groupId);
                payload.fromSlotIndex = slotIndex;
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
            if (chargesText != null)
            {
                if (action.Data.charges > 1)
                {
                    chargesText.text = action.Data.charges.ToString();
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
                    resourceCostText.text = "<b>X</b>";
                }
                //chargesLeft = action.Data.charges;
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
                    if (action.Data.manaCost <= 0 && string.IsNullOrEmpty(action.overrideResourceText))
                    {
                        resourceCostText.text = "<b>X</b>";
                    }
                    else if (action.Data.manaCost > 0 && string.IsNullOrEmpty(action.overrideResourceText))
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                    else
                    {
                        resourceCostText.text = action.overrideResourceText;
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
                    if (action.Data.manaCost <= 0 && string.IsNullOrEmpty(action.overrideResourceText))
                    {
                        resourceCostText.text = string.Empty;
                    }
                    else if (action.Data.manaCost > 0 && string.IsNullOrEmpty(action.overrideResourceText))
                    {
                        resourceCostText.text = action.Data.manaCost.ToString();
                    }
                    else
                    {
                        resourceCostText.text = action.overrideResourceText;
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
                    //macro = null;
                    break;
                case SlotKind.Action:
                    // Resolve the action for presentation (icon/name) purposes.
                    action = controller.GetResolvedAction(binding.id, ActionResolveMode.Presentation);

                    //macro = null;
                    tooltip = action.GetFullActionName();
                    break;
                case SlotKind.Macro:
                    //action = null;
                    //macro = controller.MacroLibrary.Get(binding.id);
                    //tooltip = macro.name;
                    break;
            }

            image.sprite = action.Data.icon;

            if (resourceCostText != null)
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

        public void OnClick()
        {
            if (clickHighlight == null)
                return;

            clickHighlight.transform.localScale = new Vector3(0f, 0f, 1f);
            clickHighlight.transform.LeanScale(new Vector3(1.5f, 1.5f, 1f), 0.5f);//.setOnComplete(() => { clickHighlight.LeanAlpha(0f, 0.15f); });
            Utilities.FunctionTimer.Create(this, () => clickHighlight.LeanAlpha(0f, 0.15f), 0.3f, $"{action.ActionId}_ui_click_animation_fade_delay", true, false);
            clickHighlight.LeanAlpha(1f, 0.15f);
            if (!pointer)
                selectionBorder.LeanAlpha(1f, 0.15f).setOnComplete(() => Utilities.FunctionTimer.Create(this, () => selectionBorder.LeanAlpha(0f, 0.15f), 0.2f, $"{action.ActionId}_ui_click_highlight_fade_delay", true, false));
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

            // If we have a slot this item is assigned into, forward the click event to it so it can do whatever it needs to do.
            // We need to do this because the UI layering gets really complicated otherwise.
            if (slot != null)
            {
                slot.OnPointerClick(eventData);
            }

            if (element != null)
            {
                element.onPointerClick.Invoke(new HudElementEventInfo(element, eventData));
            }

            OnClick();
        }

        public void OnDrop(PointerEventData eventData)
        {
            // If we're dragging, we don't want to trigger drop events on the item itself since it's being dropped onto something else.
            // We are only interested in drop events if we're dropping something onto this item, not if this item is being dropped onto something else.
            if (dragging)
                return;

            // If we have a slot this item is assigned into, forward the drop event to it so it can do whatever it needs to do.
            // We need to do this because the UI layering gets really complicated otherwise.
            if (slot != null)
            {
                slot.OnDrop(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }
    }
}