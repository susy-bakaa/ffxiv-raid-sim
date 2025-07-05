using UnityEngine;
using TMPro;

namespace dev.susybaka.raidsim.UI
{
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
}