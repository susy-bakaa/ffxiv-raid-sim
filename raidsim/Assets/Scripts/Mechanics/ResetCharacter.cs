// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ResetCharacter : MonoBehaviour
    {
        public CanvasGroup screenFade;
        public Vector3 location = new Vector3(0f, 1f, 0f);
        Coroutine iePerformPlayerReset;

        public void StartReset(CharacterState state)
        {
            screenFade.alpha = 0f;
            if (state.characterName.ToLower().Contains("player") && iePerformPlayerReset == null)
                iePerformPlayerReset = StartCoroutine(IE_PerformPlayerReset(state.transform));
            else if (!state.characterName.ToLower().Contains("player"))
                StartCoroutine(IE_PerformReset(state.transform));
        }

        private IEnumerator IE_PerformReset(Transform target)
        {
            yield return new WaitForSeconds(1.5f);
            target.transform.position = location;
        }

        private IEnumerator IE_PerformPlayerReset(Transform target)
        {
            yield return new WaitForSeconds(0.5f);
            screenFade.LeanAlpha(1f, 1f);
            yield return new WaitForSeconds(1f);
            target.transform.position = location;
            yield return new WaitForSeconds(0.5f);
            screenFade.LeanAlpha(0f, 2f);
            iePerformPlayerReset = null;
        }
    }
}