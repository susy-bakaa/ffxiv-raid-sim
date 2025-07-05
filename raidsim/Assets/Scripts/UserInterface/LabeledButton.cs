using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(Button))]
    public class LabeledButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Button button;
        ToggleImage toggleImage;
        TextMeshProUGUI label;

        public string[] states = new string[2] { "Turn Off", "Turn On" };

        private string originalText;

        private void Awake()
        {
            button = GetComponent<Button>();
            toggleImage = GetComponentInParent<ToggleImage>();
            label = GetComponentInChildren<TextMeshProUGUI>();
            originalText = label.text;
            button.onClick.AddListener(UpdateLabel);
            Utilities.FunctionTimer.Create(this, () => ResetLabel(), 0.5f, $"{toggleImage.gameObject.name}_LabeledButton_ResetLabel", true, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UpdateLabel();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (label != null)
            {
                label.text = originalText;
            }
        }

        private void UpdateLabel()
        {
            if (toggleImage != null && label != null && states != null && states.Length > 1)
            {
                if (toggleImage.CurrentState)
                {
                    label.text = states[0];
                }
                else
                {
                    label.text = states[1];
                }
            }
            else
            {
                label.text = originalText;
            }
        }

        public void ResetLabel()
        {
            label.text = originalText;
        }
    }
}