// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Mechanics
{
    public class DamageTriggerChild : MonoBehaviour
    {
        private DamageTrigger m_parent;

        private void Awake()
        {
            m_parent = GetComponentInParent<DamageTrigger>();
        }

        private void OnTriggerEnter(Collider other)
        {
            m_parent.OnTriggerEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            m_parent.OnTriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            m_parent.OnTriggerExit(other);
        }
    }
}