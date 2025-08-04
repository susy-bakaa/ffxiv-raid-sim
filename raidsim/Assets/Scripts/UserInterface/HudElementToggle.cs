using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementToggle : MonoBehaviour
    {
        public CanvasGroup referenceGroup;
        public CanvasGroup targetGroup;
        public bool copy;
        public bool inverse;

        private void Update()
        {
            if (referenceGroup != null && targetGroup != null)
            {
                if (copy && inverse)
                {
                    targetGroup.alpha = 1f - referenceGroup.alpha;
                }
                else if (copy)
                {
                    targetGroup.alpha = referenceGroup.alpha;
                }
            }
        }
    }
}