// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeCanvasGroup : MonoBehaviour
    {
        private CanvasGroup group;
        [SerializeField] private float fadeDuration = 0.2f;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        public void FadeToggle()
        {
            if (group.alpha < 1f)
            {
                FadeIn();
            }
            else
            {
                FadeOut();
            }
        }

        public void FadeIn()
        {
            group.LeanAlpha(1f, fadeDuration);
        }

        public void FadeOut()
        {
            group.LeanAlpha(0f, fadeDuration);
        }
    }
}