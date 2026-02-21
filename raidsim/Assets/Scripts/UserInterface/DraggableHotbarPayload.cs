// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public enum DragSourceKind { HotbarSlot, PaletteAction, PaletteMacro }

    public class DraggableHotbarPayload : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Payload")]
        public DragSourceKind sourceKind;
        public int fromSlotIndex = -1;   // if HotbarSlot
        public SlotBinding binding;      // what to place (Action/Macro + id)

        [Header("Drag visuals")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        private Transform originalParent;
        private Vector2 originalAnchoredPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalAnchoredPos = rectTransform.anchoredPosition;

            transform.SetParent(rootCanvas.transform, true);
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
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
        }

        public void ApplyDrop(HotbarController controller, int targetSlotIndex)
        {
            if (sourceKind == DragSourceKind.HotbarSlot && fromSlotIndex >= 0)
            {
                controller.SwapSlots(fromSlotIndex, targetSlotIndex);
                // You'll want to Refresh() both slot UIs; easiest is a HotbarUI root controller that refreshes all.
            }
            else
            {
                controller.SetSlot(targetSlotIndex, binding);
            }
        }
    }
}