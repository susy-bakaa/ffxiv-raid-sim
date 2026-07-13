// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class SetRotationMechanic : FightMechanic
    {
        [Header("Set Rotation Mechanic")]
        public bool resetRotationOnReset = true;
        public bool setRandomRotation = false;
        [ShowIf(nameof(setRandomRotation))] public Vector3 minRandomRotation = Vector3.zero;
        [ShowIf(nameof(setRandomRotation))] public Vector3 maxRandomRotation = Vector3.zero;
        public List<Transform> targets = new();

        private List<Quaternion> originalRotations = new();

        private void Awake()
        {
            if (targets == null || targets.Count < 1)
                targets.Add(transform);

            for (int i = 0; i < targets.Count; i++)
            {
                originalRotations.Add(targets[i].rotation);
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (targets == null || targets.Count < 1)
                return;

            if (setRandomRotation)
            {
                float x = 0f;
                float y = 0f;
                float z = 0f;

                if (minRandomRotation.x != maxRandomRotation.x)
                    x = FightTimeline.Instance.random.Range($"SetRotationMechanic_{gameObject.name.Replace(" ", string.Empty)}_RandomRotationX", minRandomRotation.x, maxRandomRotation.x);
                if (minRandomRotation.y != maxRandomRotation.y)
                    y = FightTimeline.Instance.random.Range($"SetRotationMechanic_{gameObject.name.Replace(" ", string.Empty)}_RandomRotationY", minRandomRotation.y, maxRandomRotation.y);
                if (minRandomRotation.z != maxRandomRotation.z)
                    z = FightTimeline.Instance.random.Range($"SetRotationMechanic_{gameObject.name.Replace(" ", string.Empty)}_RandomRotationZ", minRandomRotation.z, maxRandomRotation.z);

                for (int i = 0; i < targets.Count; i++)
                {
                    Transform target = targets[i];
                    target.rotation = Quaternion.Euler(x, y, z);
                }
            }
        }

        public override void InterruptMechanic(ActionInfo actionInfo)
        {
            if (resetRotationOnReset && targets != null && targets.Count > 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    Transform target = targets[i];
                    target.rotation = originalRotations[i];
                }
            }

            base.InterruptMechanic(actionInfo);
        }
    }
}