// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.Shared.Attributes
{
    public class SoundNameAttribute : PropertyAttribute
    {
        /// <summary>
        /// Relative path inside your Assets folder to the AudioManager prefab.
        /// </summary>
        public string prefabPath;

        public SoundNameAttribute(string prefabPath = "Assets/Resources/Prefabs/AudioManager.prefab")
        {
            this.prefabPath = prefabPath;
        }
    }
}