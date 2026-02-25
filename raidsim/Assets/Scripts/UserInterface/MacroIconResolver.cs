// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Actions;
using static dev.susybaka.raidsim.UI.MacroIconData;

namespace dev.susybaka.raidsim.UI
{
    public sealed class MacroIconResolver : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private MacroEditor editor;
        [SerializeField] private Sprite defaultMacroIcon;
        [SerializeField] private MacroIconData iconData; // optional
        [SerializeField] private MacroIconCatalogData iconCatalogData;
        [SerializeField] private CharacterActionRegistry registry; // for action icons
        [Header("Icon Picker")]
        [SerializeField] private PresetMacroIcon iconPrefab;
        [SerializeField] private Transform iconPageParent;
        [SerializeField] private int pages = 4;
        [SerializeField] private int iconsPerPage = 25;
        [Header("Events")]
        public UnityEvent onIconsInitialized;

        private System.Action onRefreshIconPicker;

        void Start()
        {
            Transform[] tPages = new Transform[pages];

            foreach (Transform child in iconPageParent)
            {
                tPages[child.GetSiblingIndex()] = child;
            }

            for (int i = 0; i < pages; i++)
            {
                Transform page = tPages[i];
                for (int j = 0; j < iconsPerPage; j++)
                {
                    PresetMacroIcon icon = Instantiate(iconPrefab, page);
                    int index = i * iconsPerPage + j;
                    if (index < iconData.Entries.Length)
                    {
                        icon.Initialize(editor.Library, editor, iconData.Entries[index], index, SetCustomIconForIndex);
                        onRefreshIconPicker += icon.RefreshVisuals;
                    }
                    else
                    {
                        icon.gameObject.SetActive(false);
                    }
                }
            }
            onIconsInitialized.Invoke();
        }

        public Sprite ResolveIconSprite(MacroEntry e, out CharacterAction iconAction)
        {
            iconAction = null;

            if (!e.isValid)
                return null;

            switch (e.iconMode)
            {
                case MacroIconMode.CustomSprite:
                    return iconData ? iconData.Get(e.customIconId) : defaultMacroIcon;

                case MacroIconMode.ActionIcon:
                {
                    switch (e.miconType)
                    {
                        case MacroMiconType.Action:
                        {
                            if (registry == null || string.IsNullOrWhiteSpace(e.miconName))
                            {
                                if (!string.IsNullOrEmpty(e.customIconId))
                                {
                                    return iconData ? iconData.Get(e.customIconId) : defaultMacroIcon;
                                }
                                else
                                    return defaultMacroIcon;
                            }

                            var a = registry.GetById(e.miconName);
                            var s = a != null ? a.Data.icon : null;
                            iconAction = a;

                            if (s == null && !string.IsNullOrEmpty(e.customIconId))
                            {
                                return iconData ? iconData.Get(e.customIconId) : defaultMacroIcon;
                            }

                            return s;
                        }

                        case MacroMiconType.Waymark:
                        {
                            var s = iconCatalogData ? iconCatalogData.GetWaymark(e.miconName) : null;
                            return s ? s : defaultMacroIcon;
                        }

                        case MacroMiconType.Sign:
                        {
                            var s = iconCatalogData ? iconCatalogData.GetSign(e.miconName) : null;
                            return s ? s : defaultMacroIcon;
                        }
                    }
                    return defaultMacroIcon;
                }

                default:
                    return defaultMacroIcon;
            }
        }

        // Only Action-type micon should inherit cooldown/charges visuals
        public string ResolveStateSourceActionId(MacroEntry e)
        {
            if (!e.isValid)
                return null;
            if (e.iconMode != MacroIconMode.ActionIcon)
                return null;
            if (e.miconType != MacroMiconType.Action)
                return null;

            return e.miconName; // if storing ActionId here
        }

        public int GetCustomIconIndex(Entry e)
        {
            for (int i = 0; i < iconData.Entries.Length; i++)
            {
                if (iconData.Entries[i].id == e.id)
                    return i;
            }
            return 0; // We can safely return 0 here since the default icon is always at index 0 in the icon picker
        }

        public int GetCustomIconIndex(MacroEntry e)
        {
            if (!e.isValid || e.iconMode != MacroIconMode.CustomSprite || string.IsNullOrWhiteSpace(e.customIconId))
                return 0; // We can safely return 0 here since the default icon is always at index 0 in the icon picker

            for (int i = 0; i < iconData.Entries.Length; i++)
            {
                if (iconData.Entries[i].id == e.customIconId)
                    return i;
            }
            return 0; // We can safely return 0 here since the default icon is always at index 0 in the icon picker
        }

        public void RefreshIconPicker()
        {
            onRefreshIconPicker?.Invoke();
        }

        private void SetCustomIconForIndex(int index)
        {
            editor.SetCustomIcon(iconData.Entries[index].id);
            onRefreshIconPicker?.Invoke();
        }
    }
}