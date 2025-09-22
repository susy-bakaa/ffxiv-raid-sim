// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.Shared.Attributes
{
    /// <summary>
    /// Apply to a string field to select one AssetBundle name
    /// via a dropdown of all bundles in the project.
    /// </summary>
    public class AssetBundleNameAttribute : PropertyAttribute { }
}