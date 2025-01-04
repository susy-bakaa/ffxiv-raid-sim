using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionDisplay : MonoBehaviour
{
    TextMeshProUGUI display;

    private void Awake()
    {
        display = GetComponent<TextMeshProUGUI>();
        display.text = $"Version: {Application.version}";
    }
}
