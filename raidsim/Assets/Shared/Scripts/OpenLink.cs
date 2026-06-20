// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace dev.susybaka.Shared
{
    public class OpenLink : MonoBehaviour
    {
        [SerializeField] private bool multiple = false;
        [SerializeField, ShowIf(nameof(multiple))] private int currentActiveLink = 0;
        [SerializeField, HideIf(nameof(multiple))] private string link = string.Empty;
        [SerializeField, ShowIf(nameof(multiple))] private string[] links = new string[0];

        private Button button;
        private Coroutine ieOpenDelay;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (!multiple)
                links = new string[] { link };
        }

        public void SetActiveLink(int index)
        {
            currentActiveLink = Mathf.Clamp(index, 0, links.Length - 1);
        }

        public void Open(int index)
        {
            OpenDelayed(index);
        }

        public void Open()
        {
            OpenDelayed(currentActiveLink);
        }

        private void OpenDelayed(int index)
        {
            if (ieOpenDelay == null)
            {
                ieOpenDelay = StartCoroutine(IE_OpenDelay(new WaitForSecondsRealtime(0.5f), index));
                button.interactable = false;
            }
        }

        private IEnumerator IE_OpenDelay(WaitForSecondsRealtime wait, int index)
        {
            yield return wait;
            OpenInternal(index);
            button.interactable = true;
            ieOpenDelay = null;
        }

        private void OpenInternal(int index)
        {
            if (multiple && links != null && links.Length > 0)
            {
                link = links[Mathf.Clamp(index, 0, links.Length - 1)];
            }

            if (string.IsNullOrEmpty(link))
            {
                Debug.LogWarning("Link is not set.");
                return;
            }

            Application.OpenURL(link);
        }
    }
}