// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class SetObjectActive : MonoBehaviour
    {
        public GameObject objectToSet;
        public bool setChildrenToo;

        public void SetActiveTo(bool active)
        {
            SetObjectActiveTo(objectToSet, active);
        }

        private void SetObjectActiveTo(GameObject obj, bool active)
        {
            obj.SetActive(active);

            if (setChildrenToo)
            {
                foreach (Transform child in obj.transform)
                {
                    SetObjectActiveTo(child.gameObject, active);
                }
            }
        }
    }
}