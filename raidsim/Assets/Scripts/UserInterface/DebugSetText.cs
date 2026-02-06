// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.UI.Development
{
    public class DebugSetText : MonoBehaviour
    {
        public string textLabelToSet = string.Empty;
        public string setToText = string.Empty;

        private TextMeshProUGUI textLabel;

        private void Awake()
        {
            textLabel = Utilities.FindAnyByName(textLabelToSet).GetComponentInChildren<TextMeshProUGUI>(true);
            textLabel.text = string.Empty;
        }

        public void SetText(CharacterState state)
        {
            SetText(new ActionInfo(null, state, state));
        }

        public void SetText(ActionInfo actionInfo)
        {
            if (actionInfo.target != null && actionInfo.targetIsPlayer)
            {
                SetText(setToText);
            }
            else if (actionInfo.source != null && actionInfo.sourceIsPlayer)
            {
                SetText(setToText);
            }
        }

        public void SetText(string text)
        {
            if (textLabel != null)
            {
                textLabel.text = text;
            }
        }

        public void ClearText()
        {
            if (textLabel != null)
            {
                textLabel.text = string.Empty;
            }
        }
    }
}