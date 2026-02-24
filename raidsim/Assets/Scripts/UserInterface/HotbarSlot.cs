// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.EventSystems;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.Inputs.UserInput;
using TMPro;

namespace dev.susybaka.raidsim.UI
{
    public class HotbarSlot : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        private UserInput input;

        [SerializeField] private string groupId;
        [SerializeField] private int slotIndex;
        [SerializeField] private Hotbar hotbar;
        [SerializeField] private HotbarItem itemPrefab;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private TextMeshProUGUI keybindText;
        [SerializeField] private KeyBind keybind;

        public string GroupId => groupId;
        public int SlotIndex => slotIndex;

        private HotbarItem current;

        private void OnEnable() 
        {
            if (!hotbar)
                hotbar = transform.GetComponentInParents<Hotbar>();

            if (!input)
                input = FightTimeline.Instance.input;

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
            // Update the visual text on the slot to show the current keybind
            if (keybindText != null)
            {
                keybind = null;
                string key = hotbar.Controller.GetGroupKeybind(groupId, slotIndex);

                foreach (var kbind in input.keys)
                {
                    if (kbind.name == key)
                    {
                        keybind = kbind.bind;
                        break;
                    }
                }

                if (keybind != null)
                {
                    keybindText.text = keybind.ToShortString();
                }
                else
                {
                    keybindText.text = string.Empty;
                }
            }

            // Update the item in the slot based on the current binding
            var binding = hotbar.Controller.GetBinding(groupId, slotIndex);

            if (binding.kind == SlotKind.Empty)
            {
                if (current)
                {
                    hotbar.Controller.DetachFromGroupKeybind(groupId, ClickCurrentItem); // Unregister the click callback from the controller
                    Destroy(current.gameObject);
                }
                current = null;
                return;
            }

            if (!current)
                current = Instantiate(itemPrefab, contentRoot);
            else
                hotbar.Controller.DetachFromGroupKeybind(groupId, ClickCurrentItem); // Unregister the click callback from the controller before rebinding

            current.Bind(hotbar.Controller, hotbar.Controller.MacroEditor, this, binding);
            hotbar.Controller.AttachToGroupKeybind(groupId, ClickCurrentItem); // Register the click callback with the controller so it can be invoked when the keybind is pressed
        }

        public void RefreshStatic()
        {
            if (current != null)
                current.RefreshStaticVisuals();
        }

        public void ClickCurrentItem(int index)
        {
            if (!current || index != slotIndex)
                return;

            current.OnClick();
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
            //Debug.Log($"Dropping {drag.sourceKind} from slot {drag.fromSlotIndex} to hotbar slot {slotIndex}");
            drag.ApplyDrop(hotbar.Controller, groupId, slotIndex);
            hotbar.RefreshSlots(); // Update all slots since some bindings may have changed due to swapping
        }
    }
}