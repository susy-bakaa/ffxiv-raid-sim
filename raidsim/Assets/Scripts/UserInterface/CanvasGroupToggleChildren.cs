using UnityEngine;

namespace dev.susybaka.raidsim
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupToggleChildren : MonoBehaviour
    {
        private CanvasGroup group;
        public CanvasGroup Group => group;
        private bool done = false;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (group == null)
                return;

            if (!done && group.alpha > 0f)
            {
                done = true;
                foreach (Transform child in group.transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else if (done && group.alpha <= 0f)
            {
                done = false;
                foreach (Transform child in group.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateState()
        {
            if (group == null)
                return;

            if (group.alpha > 0f)
            {
                done = true;
                foreach (Transform child in group.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            else if (group.alpha <= 0f)
            {
                done = false;
                foreach (Transform child in group.transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
}