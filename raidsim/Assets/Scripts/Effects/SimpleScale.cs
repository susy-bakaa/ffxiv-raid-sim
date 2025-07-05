using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleScale : MonoBehaviour
    {
        public Vector3 scale;
        public bool setScale = false;
        public bool onUpdate = true;
        public float duration = 0.5f;

        private Vector3 originalScale;
        private bool scaled;

        private void Awake()
        {
            originalScale = transform.localScale;
            scaled = false;
        }

        private void Update()
        {
            if (!onUpdate)
                return;

            Scale();
        }

        public void Scale()
        {
            if (scaled)
                return;

            if (!setScale)
            {
                if (scale != Vector3.zero)
                {
                    scaled = true;
                    transform.LeanScale(scale, duration);
                }
            }
            else
            {
                scaled = true;
                transform.localScale = scale;
            }
        }

        public void ResetScale()
        {
            if (!scaled)
                return;

            if (!setScale)
            {
                if (originalScale != Vector3.zero)
                {
                    scaled = false;
                    transform.LeanScale(originalScale, duration);
                }
                else
                {
                    scaled = false;
                    originalScale = new Vector3(0.01f, 0.01f, 0.01f);
                    transform.localScale = originalScale;
                }
            }
            else
            {
                scaled = false;
                transform.localScale = originalScale;
            }
        }
    }
}