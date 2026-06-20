// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.Events
{
    public class OverlapTrigger : MonoBehaviour
    {
        [SerializeField] private bool log = false;
        [SerializeField] private Collider[] ignoreColliders = new Collider[0];
        [SerializeField, Tag] private string tagFilter = string.Empty;
        [SerializeField] private UnityEvent<Collider> onOverlap;

        void OnTriggerEnter(Collider other)
        {
            if (System.Array.Exists(ignoreColliders, c => c == other))
                return;

            if (!other.CompareTag(tagFilter))
                return;

            if (log)
                Debug.Log($"[OverlapTrigger ({gameObject.name})] Detected overlap with {other.gameObject.name}!", other.gameObject);

            onOverlap.Invoke(other);
        }
    }
}