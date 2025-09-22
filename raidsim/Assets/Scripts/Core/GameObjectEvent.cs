// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.Events
{
    public class GameObjectEvent : MonoBehaviour
    {
        public GameObject target;
        public UnityEvent<GameObject> m_event;

        private void Awake()
        {
            if (target == null)
            {
                target = gameObject;
            }
        }

        public void BasicGameObjectEvent()
        {
            m_event.Invoke(target);
        }
    }
}