// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleRotation : MonoBehaviour
    {
        public Vector3 rotation;
        public bool setRotation = false;
        public string faceTowardsName;
        public Transform faceTowards;

        private void Awake()
        {
            if (!string.IsNullOrEmpty(faceTowardsName))
            {
                faceTowards = Utilities.FindAnyByName(faceTowardsName)?.transform;
            }
        }

        private void Update()
        {
            if (faceTowards == null)
            {
                if (!setRotation)
                {
                    if (rotation != Vector3.zero)
                    {
                        transform.Rotate(rotation * Time.deltaTime);
                    }
                }
                else
                {
                    transform.eulerAngles = rotation;
                }
            }
            else
            {
                transform.LookAt(faceTowards);
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            }
        }
    }
}