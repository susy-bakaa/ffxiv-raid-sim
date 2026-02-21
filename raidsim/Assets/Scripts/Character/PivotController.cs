// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.Characters
{
    public class PivotController : MonoBehaviour
    {
        [SerializeField] private Transform mainPivot;
        [SerializeField] private Transform dashKnockbackPivot;
        [SerializeField] private Transform groundTargetingPivot;

        public Transform MainPivot => mainPivot;
        public Transform DashKnockbackPivot => dashKnockbackPivot;
        public Transform GroundTargetingPivot => groundTargetingPivot;

        private void Awake()
        {
            if (mainPivot == null)
                mainPivot = transform.GetChild(0);
            if (dashKnockbackPivot == null)
                dashKnockbackPivot = transform.Find("DashKnockbackPivot");
            if (groundTargetingPivot == null)
                groundTargetingPivot = transform.Find("GroundTargetingPivot");
        }
    }
}