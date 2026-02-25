// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    [CreateAssetMenu(fileName = "Macro Icon Catalog Data", menuName = "FFXIV/New Macro Icon Catalog Data")]
    public sealed class MacroIconCatalogData : ScriptableObject
    {
        [Serializable]
        public struct WaymarkIcon
        {
            public string name;   // "A","B","C","D","1","2","3","4"
            [NaughtyAttributes.ShowAssetPreview] public Sprite sprite;
        }
        [Serializable]
        public struct SignIcon
        {
            public string name;   // "Attack1..8","Bind1..3","Ignore1..2","Circle","Cross","Triangle","Square"
            [NaughtyAttributes.ShowAssetPreview] public Sprite sprite;
        }

        [SerializeField] private WaymarkIcon[] waymarks;
        [SerializeField] private SignIcon[] signs;

        public Sprite GetWaymark(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            for (int i = 0; i < waymarks.Length; i++)
                if (string.Equals(waymarks[i].name, name, StringComparison.OrdinalIgnoreCase))
                    return waymarks[i].sprite;
            return null;
        }

        public Sprite GetSign(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            for (int i = 0; i < signs.Length; i++)
                if (string.Equals(signs[i].name, name, StringComparison.OrdinalIgnoreCase))
                    return signs[i].sprite;
            return null;
        }
    }
}