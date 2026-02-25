// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldHelper : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI characterCount;
        [SerializeField] private TextMeshProUGUI lineCount;
        public UnityEvent onTextContentChangedWhileActive;

        private int lastTextLength;
        private string lastTextContent;

        private void Awake()
        {
            if (inputField == null)
            {
                inputField = GetComponent<TMP_InputField>();
            }
            UpdateDisplay();
        }

        private void Update()
        {
            if (inputField.isFocused)
            {
                if (lastTextLength != inputField.text.Length || lastTextContent != inputField.text)
                {
                    lastTextContent = inputField.text;
                    lastTextLength = inputField.text.Length;
                    onTextContentChangedWhileActive?.Invoke();
                }
            }
        }

        public void UpdateDisplay()
        {
            if (inputField == null)
                return;

            if (characterCount != null)
            {
                characterCount.text = $"{inputField.text.Length}/{inputField.characterLimit}";
            }
            if (lineCount != null)
            {
                // Handle empty text case to avoid counting 1 line when there is technically one line but no characters
                // Rest of the time, count lines by splitting on newline characters, so even empty lines are counted correctly
                int lines = string.IsNullOrEmpty(inputField.text) ? 0 : inputField.text.Split('\n').Length;
                lineCount.text = $"{lines}/{inputField.lineLimit}";
            }
        }
    }
}