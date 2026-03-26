// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class BusyText : MonoBehaviour
    {
        [SerializeField] private string[] busyTexts;
        [SerializeField] private float timerGoal = 1f;

        private TMPro.TextMeshProUGUI label;
        private float timer = 0f;
        private int currentTextIndex = 0;

        private void Awake()
        {
            label = GetComponent<TMPro.TextMeshProUGUI>();
        }

        private void Update()
        {
            if (busyTexts == null || busyTexts.Length < 1)
                return;

            timer += Time.unscaledDeltaTime;

            if (timer >= timerGoal)
            {
                currentTextIndex = (currentTextIndex + 1) % busyTexts.Length;
                label.text = busyTexts[currentTextIndex];
                timer = 0f;
            }
        }
    }
}