// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace dev.susybaka.raidsim
{
    public class DisableInBuild : MonoBehaviour
    {
        Button button;
        Toggle toggle;
        TMP_Dropdown dropdown;
        public bool disableForWebGL = true;
        public bool disableForWinStandalone = false;
        public bool disableForLinuxStandalone = false;
        public bool toggleGameObject = false;

        private void Awake()
        {
            button = GetComponent<Button>();
            toggle = GetComponent<Toggle>();
            dropdown = GetComponent<TMP_Dropdown>();

#if UNITY_WEBPLAYER
        if (disableForWebGL)
        {
            if (button != null)
                button.interactable = false;
            else if (toggle != null)
                toggle.interactable = false;
            else if (dropdown != null)
                dropdown.interactable = false;
            if (toggleGameObject)
                gameObject.SetActive(false);
        }
#endif
#if UNITY_STANDALONE_LINUX
        if (disableForLinuxStandalone)
        {
            if (button != null)
                button.interactable = false;
            else if (toggle != null)
                toggle.interactable = false;
            else if (dropdown != null)
                dropdown.interactable = false;
            if (toggleGameObject)
                gameObject.SetActive(false);
        }
#endif
#if UNITY_STANDALONE_WIN
            if (disableForWinStandalone)
            {
                if (button != null)
                    button.interactable = false;
                else if (toggle != null)
                    toggle.interactable = false;
                else if (dropdown != null)
                    dropdown.interactable = false;
                if (toggleGameObject)
                    gameObject.SetActive(false);
            }
#endif
        }
    }
}