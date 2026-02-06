// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Mechanics;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleTetherEffect : MonoBehaviour
    {
        TetherTrigger tether;

        public ParticleSystem[] effects;
        public bool hideOnStart = true;
        public bool followTarget = true;
        [ShowIf(nameof(followTarget))] public bool followEndpoint = false;
        [ShowIf(nameof(followTarget))] public bool followStartpoint = false;
        [ShowIf(nameof(followTarget))] public Vector3 positionOffset;
        [ShowIf(nameof(followTarget))] public bool localOffset = true;

        private bool visible = false;

        private void Awake()
        {
            if (hideOnStart)
            {
                SetVisible(false);
            }
        }

        private void Update()
        {
            if (tether == null)
                return;

            if (followTarget)
            {
                if (followEndpoint)
                {
                    transform.position = tether.tetherTarget.pivot.position + tether.targetOffset;
                }
                else if (followStartpoint)
                {
                    transform.position = tether.tetherSource.pivot.position + tether.sourceOffset;
                }

                if (localOffset)
                    transform.localPosition = transform.localPosition + positionOffset;
                else
                    transform.position = transform.position + positionOffset;
            }
        }

        public void Initialize(TetherTrigger tether)
        {
            this.tether = tether;
        }

        public void SetVisible(bool visible)
        {
            if (this.visible == visible)
                return;

            this.visible = visible;

            if (visible)
            {
                foreach (var effect in effects)
                {
                    effect.Play();
                }
            }
            else
            {
                foreach (var effect in effects)
                {
                    effect.Stop();
                }
            }
        }
    }
}