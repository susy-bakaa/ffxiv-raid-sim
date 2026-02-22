// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        [SerializeField] private string groupId;
        [SerializeField] private int slotIndex;
        [SerializeField] private Hotbar hotbar;
        [SerializeField] private HotbarItem itemPrefab;
        [SerializeField] private Transform contentRoot;

        public string GroupId => groupId;
        public int SlotIndex => slotIndex;

        private HotbarItem current;

        private void OnEnable() 
        {
            if (!hotbar)
                hotbar = transform.GetComponentInParents<Hotbar>();

            hotbar.RegisterSlot(this);

            Refresh(); 
        }

        private void OnDisable() 
        {
            hotbar.UnregisterSlot(this);
        }

        public void SetGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return;

            this.groupId = groupId; 
            Refresh();
        }

        public void Refresh()
        {
            var binding = hotbar.Controller.GetBinding(groupId, slotIndex);

            if (binding.kind == SlotKind.Empty)
            {
                if (current)
                    Destroy(current.gameObject);
                current = null;
                return;
            }

            if (!current)
                current = Instantiate(itemPrefab, contentRoot);

            current.Bind(hotbar.Controller, this, binding);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                hotbar.Controller.ExecuteSlot(groupId, slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                hotbar.Controller.ClearSlot(groupId, slotIndex);
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
            Debug.Log($"Dropping {drag.sourceKind} from slot {drag.fromSlotIndex} to hotbar slot {slotIndex}");
            drag.ApplyDrop(hotbar.Controller, groupId, slotIndex);
            hotbar.RefreshSlots(); // Update all slots since some bindings may have changed due to swapping
        }
    }
}