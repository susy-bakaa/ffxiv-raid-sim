// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class SetActive : MonoBehaviour
    {
        [SerializeField] private GameObject[] gameObjects;
        [SerializeField] private bool setOnStart = true;
        [SerializeField] private bool state;
        [SerializeField] private float delay = 0.1f;

        private Coroutine ieSetActive;
        private bool ranStart = false;

        private void Start()
        {
            if (!setOnStart)
            {
                ranStart = true;
                return;
            }

            SetStateInternal();
        }

        public void SetState(bool state)
        {
            if (!ranStart)
                return;

            this.state = state;
            SetStateInternal();
        }

        public void ToggleState()
        {
            if (!ranStart)
                return;

            this.state = !this.state;
            SetStateInternal();
        }

        private void SetStateInternal()
        {
            if (delay > 0f)
            {
                if (ieSetActive == null)
                    ieSetActive = StartCoroutine(IE_SetActive(new WaitForSeconds(delay)));
            }
            else
            {
                ranStart = true;
                SetActiveInternal();
            }
        }

        private IEnumerator IE_SetActive(WaitForSeconds wait)
        {
            yield return wait;
            ranStart = true;
            SetActiveInternal();
            ieSetActive = null;
        }

        private void SetActiveInternal()
        {
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(state);
            }
        }
    }
}