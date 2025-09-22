// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

// Does not really work at the moment, could not really get it to work and wrote this behaviour into the custom editor of FightSelector instead,
// but kept this attribute here in case anyone can get it to work properly in the future. I am just not smart enough to figure it out right now.
namespace dev.susybaka.Shared.Attributes
{
    /// <summary>
    /// Apply to a string[] or List<string> field to select multiple AssetBundle names
    /// via a dropdown checklist of all bundles in the project.
    /// </summary>
    public class AssetBundleNamesAttribute : PropertyAttribute
    {
        public AssetBundleNamesAttribute() { }
    }
}