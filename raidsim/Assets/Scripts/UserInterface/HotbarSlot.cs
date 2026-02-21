// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private HotbarController controller;
        [SerializeField] private HotbarItem itemPrefab;
        [SerializeField] private Transform contentRoot;

        private HotbarItem current;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            var binding = controller.GetBinding(slotIndex);

            if (binding.kind == SlotKind.Empty)
            {
                if (current)
                    Destroy(current.gameObject);
                current = null;
                return;
            }

            if (!current)
                current = Instantiate(itemPrefab, contentRoot);

            current.Bind(controller, slotIndex, binding);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                controller.ExecuteSlot(slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                controller.ClearSlot(slotIndex);
                Refresh();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag)
                return;
            var drag = eventData.pointerDrag.GetComponent<DraggableHotbarPayload>();
            if (!drag)
                return;

            drag.ApplyDrop(controller, slotIndex);
            Refresh(); // and also refresh any source slot UI (see below)
        }
    }
}