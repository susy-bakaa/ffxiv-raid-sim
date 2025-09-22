// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class HideSetting : MonoBehaviour
    {
        public Toggle toggle;
        public GameObject target;

        private void Update()
        {
            if (toggle.isOn)
            {
                target.SetActive(true);
            }
            else
            {
                target.SetActive(false);
            }
        }
    }
}