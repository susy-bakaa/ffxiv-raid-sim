// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Actions;

namespace dev.susybaka.raidsim.UI
{
    public sealed class MacroIconResolver : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private MacroEditor editor;
        [SerializeField] private Sprite defaultMacroIcon;
        [SerializeField] private MacroIconData iconData; // optional
        [SerializeField] private CharacterActionRegistry registry; // for action icons
        [Header("Icon Picker")]
        [SerializeField] private PresetMacroIcon iconPrefab;
        [SerializeField] private Transform iconPageParent;
        [SerializeField] private int pages = 4;
        [SerializeField] private int iconsPerPage = 25;

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
                        icon.Initialize(editor.Library, editor, iconData.Entries[index], index, () => editor.SetCustomIcon(iconData.Entries[index].id));
                    }
                    else
                    {
                        icon.gameObject.SetActive(false);
                    }
                }
            }
        }

        public Sprite ResolveIconSprite(MacroEntry e)
        {
            if (!e.isValid)
                return null;

            switch (e.iconMode)
            {
                case MacroIconMode.CustomSprite:
                    return iconData ? iconData.Get(e.customIconId) : defaultMacroIcon;

                case MacroIconMode.ActionIcon:
                {
                    if (registry == null || string.IsNullOrWhiteSpace(e.actionIconId))
                        return defaultMacroIcon;

                    var a = registry.GetById(e.actionIconId);
                    var s = a != null ? a.Data.icon : null;
                    return s ? s : defaultMacroIcon;
                }

                default:
                    return defaultMacroIcon;
            }
        }

        public string ResolveStateSourceActionId(MacroEntry e)
        {
            if (!e.isValid)
                return null;
            return e.iconMode == MacroIconMode.ActionIcon ? e.actionIconId : null;
        }
    }
}