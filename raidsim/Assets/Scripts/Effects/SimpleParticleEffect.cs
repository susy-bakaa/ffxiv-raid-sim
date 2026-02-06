// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleParticleEffect : MonoBehaviour
    {
        public ParticleSystem[] particles;
        public bool playOnStart = false;

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
            else
            {
                foreach (ParticleSystem p in particles)
                {
                    p.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        public void Play()
        {
            foreach (ParticleSystem p in particles)
            {
                p.Play(false);
            }
        }

        public void Stop()
        {
            foreach (ParticleSystem p in particles)
            {
                p.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}