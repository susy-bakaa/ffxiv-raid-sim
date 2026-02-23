// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI
{
    public enum DragSourceKind { HotbarSlot, PaletteAction, PaletteMacro }

    public class DraggableHotbarPayload : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private UserInput input;
        public HotbarController controller;
        [SerializeField] private bool ignoreLock = false;

        [Header("Payload")]
        public DragSourceKind sourceKind;
        public string fromGroupId = string.Empty;
        public int fromPageIndex = -1;
        public int fromSlotIndex = -1;
        public SlotBinding binding;

        [Header("Drag visuals")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        private Transform originalParent;
        private Vector2 originalAnchoredPos;
        private bool dragging = false;

        private void Awake()
        {
            if (rootCanvas == null)
                rootCanvas = transform.GetComponentInParents<Canvas>();
            input = FightTimeline.Instance.input;
        }

        void Update()
        {
            if (dragging)
            {
                input.rotationInputEnabled = false;
                input.zoomInputEnabled = false;
                input.targetRaycastInputEnabled = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalAnchoredPos = rectTransform.anchoredPosition;

            // Don't allow dragging if the source is locked, unless ignoreLock is true (for example, dragging from the palette should be allowed even if the hotbar is locked).
            if (controller != null && !ignoreLock && controller.locked)
                return;

            transform.SetParent(rootCanvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (controller != null && !ignoreLock && controller.locked)
                return;

            rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            // If nothing accepted the drop, snap back
            if (transform.parent == rootCanvas.transform)
            {
                transform.SetParent(originalParent, false);
                rectTransform.anchoredPosition = originalAnchoredPos;
            }
            dragging = false;
            input.rotationInputEnabled = true;
            input.zoomInputEnabled = true;
            input.targetRaycastInputEnabled = true;
        }

        public void ApplyDrop(HotbarController controller, string targetGroupId, int targetSlotIndex)
        {
            if (sourceKind == DragSourceKind.HotbarSlot && fromSlotIndex >= 0)
            {
                controller.SwapSlots(fromGroupId, fromPageIndex, fromSlotIndex, targetGroupId, controller.GetActivePage(targetGroupId), targetSlotIndex);
                // You'll want to Refresh() both slot UIs; easiest is a HotbarUI root controller that refreshes all.
            }
            else
            {
                controller.SetSlot(targetGroupId, targetSlotIndex, binding);
            }
        }
    }
}