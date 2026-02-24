// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using dev.susybaka.raidsim.Actions;
using dev.susybaka.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI
{
    public sealed class MacroSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int macroIndex;
        [SerializeField] private MacroEditor editor;
        [SerializeField] private HotbarController hotbarController;

        [Header("UI")]
        [SerializeField] private PresetHotbarItem hotbarItem;
        [SerializeField] private Transform contentParent;
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private GameObject selectionOutline;

        public System.Action<int> OnEditRequested;

        private PresetHotbarItem current;
        private bool subbed = false;

        private void OnEnable()
        {
            if (editor != null && editor.Library != null)
            {
                if (!subbed)
                {
                    editor.Library.OnMacroChanged += HandleChanged;
                    Refresh();
                    subbed = true;
                }
            }
        }

        private void OnDisable()
        {
            if (subbed)
            {
                editor.Library.OnMacroChanged -= HandleChanged;
                subbed = false;
            }
        }

        public void Initialize(MacroEditor editor, HotbarController hotbarController, int index)
        {
            this.editor = editor;
            this.hotbarController = hotbarController;
            this.macroIndex = index;
            if (indexText != null )
                indexText.text = index.ToString();

            if (!subbed)
            {
                editor.Library.OnMacroChanged += HandleChanged;
                Refresh();
                subbed = true;
            }
        }

        public void HandleChanged(int idx)
        {
            //Debug.Log($"MacroSlot {macroIndex} received change notification for index {idx}");

            if (idx == macroIndex)
            {
                editor.UpdateIconPreview(current);
                Refresh();
            }
            if (selectionOutline != null)
            {
                //Debug.Log($"Updating selection outline for MacroSlot {macroIndex}: {(idx == macroIndex ? "selected" : "not selected")}");
                selectionOutline.SetActive(idx == macroIndex);
            }
        }

        public void Refresh()
        {
            var e = editor.Library.Get(macroIndex);

            if (current != null && !e.isValid)
            {
                Destroy(current.gameObject);
            }
            else if (e.isValid)
            {
                if (current == null)
                {
                    current = Instantiate(hotbarItem, contentParent);
                }
            }

            if (current != null)
                current.Initialize(hotbarController, editor, new SlotBinding { id = MacroLibrary.MacroIdFromIndex(macroIndex), kind = SlotKind.Macro});
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"MacroSlot {macroIndex} clicked with button {eventData.button}");
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnEditRequested?.Invoke(macroIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                editor.Library.Clear(macroIndex);
            }
        }
    }
}