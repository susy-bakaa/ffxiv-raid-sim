// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class SetActiveLinker : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private bool inverse = false;

        private void OnEnable()
        {
            if (inverse)
                target.SetActive(false);
            else
                target.SetActive(true);
        }

        private void OnDisable()
        {
            if (inverse)
                target.SetActive(true);
            else
                target.SetActive(false);
        }
    }
}