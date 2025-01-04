using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    }
}
