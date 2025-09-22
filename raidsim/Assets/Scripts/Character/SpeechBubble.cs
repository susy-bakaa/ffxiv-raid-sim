// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Characters
{
    public class SpeechBubble : MonoBehaviour
    {
        CharacterState characterState;

        public float normalHeight = 75f;
        public float nameplateHiddenHeight = 25f;

        private void Awake()
        {
            characterState = transform.GetComponentInParents<CharacterState>();
        }

        private void Update()
        {
            if (characterState == null)
                return;

            if (Utilities.RateLimiter(47))
            {
                if (characterState != null)
                {
                    if (characterState.hideNameplate)
                    {
                        transform.localPosition = new Vector3(0, nameplateHiddenHeight, 0);
                    }
                    else
                    {
                        transform.localPosition = new Vector3(0, normalHeight, 0);
                    }
                }
            }
        }
    }
}