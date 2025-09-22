// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class DestroyObjects : MonoBehaviour
    {
        public List<string> objectNames = new List<string>();

        public void TriggerDestruction()
        {
            for (int i = 0; i < objectNames.Count; i++)
            {
                Destroy(GameObject.Find(objectNames[i]));
            }
        }
    }
}