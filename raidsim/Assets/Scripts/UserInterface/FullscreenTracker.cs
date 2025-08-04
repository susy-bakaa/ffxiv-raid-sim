using UnityEngine;
using TMPro;

namespace dev.susybaka.raidsim.UI.Development
{
    public class FullscreenTracker : MonoBehaviour
    {
        private TextMeshProUGUI tm;
        [SerializeField] private bool trackModeInstead = false;

        private void Awake()
        {
            tm = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (trackModeInstead)
                tm.text = "Fullscreen Mode: " + Screen.fullScreenMode.ToString();
            else
                tm.text = "Fullscreen: " + Screen.fullScreen.ToString();
        }
    }
}