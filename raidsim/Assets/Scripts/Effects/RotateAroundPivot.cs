// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class RotateAroundPivot : MonoBehaviour
    {
        public Transform pivot;        // The object to rotate around
        public float distance = 5f;    // The fixed distance from the pivot
        public float speed = 50f;      // Speed of rotation (degrees per second)
        public bool clockwise = true;  // Whether to rotate clockwise or counterclockwise

        private Vector3 offset;

        private void Start()
        {
            if (pivot == null)
            {
                Debug.LogError("Pivot object is not assigned.");
                return;
            }

            // Initialize the offset to maintain the fixed distance
            offset = transform.position - pivot.position;
            offset = offset.normalized * distance;
        }

        private void Update()
        {
            if (pivot == null)
                return;

            // Calculate rotation direction
            float rotationDirection = clockwise ? -1f : 1f;

            // Rotate the offset vector around the pivot's up axis
            Quaternion rotation = Quaternion.Euler(0, rotationDirection * speed * Time.deltaTime, 0);
            offset = rotation * offset;

            // Update the object's position
            transform.position = pivot.position + offset;

            // Make the object face the rotation direction (tangent to its path)
            Vector3 tangent = Vector3.Cross(offset, pivot.up).normalized;
            transform.rotation = Quaternion.LookRotation(clockwise ? -tangent : tangent, Vector3.up);
        }
    }
}