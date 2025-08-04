using UnityEngine;
using UnityEngine.UI;

namespace dev.susybaka.raidsim.UI
{
    public class HideSetting : MonoBehaviour
    {
        public Toggle toggle;
        public GameObject target;

        private void Update()
        {
            if (toggle.isOn)
            {
                target.SetActive(true);
            }
            else
            {
                target.SetActive(false);
            }
        }
    }
}