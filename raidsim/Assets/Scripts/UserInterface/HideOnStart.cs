// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HideOnStart : MonoBehaviour
    {
        CanvasGroup group;

        public bool repeatOnReset = false;
        public bool showAfterDelay = false;
        [ShowIf("showAfterDelay")] public float delay = 1f;

        Coroutine ieShowAfterDelay;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();

            Hide();
        }

        private void OnEnable()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.AddListener(OnReset);
            }
        }

        private void OnDisable()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnDestroy()
        {
            if (repeatOnReset && FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onReset.RemoveListener(OnReset);
            }
        }

        private void OnReset()
        {
            if (!repeatOnReset)
                return;

            StopAllCoroutines();
            ieShowAfterDelay = null;
            Hide();
        }

        private void Hide()
        {
            if (group != null)
            {
                group.alpha = 0;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (showAfterDelay && delay > 0f)
            {
                ieShowAfterDelay = StartCoroutine(IE_ShowAfterDelay(new WaitForSecondsRealtime(delay)));
            }
        }

        private IEnumerator IE_ShowAfterDelay(WaitForSecondsRealtime wait)
        {
            yield return wait;
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
                group.interactable = true;
            }
            ieShowAfterDelay = null;
        }
    }
}