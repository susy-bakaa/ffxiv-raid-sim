// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Visuals
{
    public class FollowRotation : MonoBehaviour
    {
        public Axis freeze;
        public bool setAutomatically = true;
        public Transform target;
        public Vector3 targetRotation;
        [Space(20)]
        // Deprecated but still here for backwards compatibility with old scenes,
        // because I am not hunting down all of these components and changing them
        [NaughtyAttributes.InfoBox("DEPRECATED, use the 'freeze' variable instead. These are kept for backwards compatability.", NaughtyAttributes.EInfoBoxType.Warning)]
        public bool freezeX;
        public bool freezeY;
        public bool freezeZ;

        private void Awake()
        {
            if (setAutomatically)
            {
                targetRotation = transform.rotation.eulerAngles;
            }
        }

        private void Update()
        {
            if (setAutomatically && target != null)
            {
                targetRotation = target.rotation.eulerAngles;
            }

            Vector3 currentEulerAngles = transform.rotation.eulerAngles;

            if (freeze.x || freezeX)
                currentEulerAngles.x = targetRotation.x;
            if (freeze.y || freezeY)
                currentEulerAngles.y = targetRotation.y;
            if (freeze.z || freezeZ)
                currentEulerAngles.z = targetRotation.z;

            transform.rotation = Quaternion.Euler(currentEulerAngles);
        }
    }
}