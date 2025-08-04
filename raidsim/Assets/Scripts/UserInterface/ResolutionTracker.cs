using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.UI.Development
{
    public class ResolutionTracker : MonoBehaviour
    {
        private TextMeshProUGUI tm;
        [SerializeField] private bool useCurrentGameResolution = true;

        private void Awake()
        {
            tm = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (useCurrentGameResolution)
                tm.text = "Resolution Game: " + GlobalVariables.currentGameResolution.width + "x" + GlobalVariables.currentGameResolution.height + " " + GlobalVariables.currentGameResolution.refreshRateRatio;
            else
                tm.text = "Resolution Screen: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + " " + Screen.currentResolution.refreshRateRatio;
        }
    }
}