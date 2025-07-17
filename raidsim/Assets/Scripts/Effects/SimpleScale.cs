using System.Collections;
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleScale : MonoBehaviour
    {
        public Vector3 scale;
        public bool setScale = false;
        public bool onUpdate = true;
        public bool overrideOriginalScale = false;
        [NaughtyAttributes.ShowIf("overrideOriginalScale")] public Vector3 originalScale = Vector3.zero;
        public float duration = 0.5f;

        private bool scaled;

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private bool _setReset = true;
        [SerializeField] private float _holdTime = 2f;
        [NaughtyAttributes.Button("Scale")]
        private void ScaleEditor()
        {
            Vector3 _originalScale = transform.localScale;

            if (!setScale)
            {
                if (scale != Vector3.zero)
                {
                    transform.LeanScale(scale, duration);
                }
            }
            else
            {
                transform.localScale = scale;
            }
            StartCoroutine(IE_ScaleEditor(new WaitForSecondsRealtime(_holdTime), _originalScale));
        }

        private IEnumerator IE_ScaleEditor(WaitForSecondsRealtime wait, Vector3 originalScale)
        {
            yield return wait;
            if (!_setReset)
            {
                transform.LeanScale(originalScale, duration);
            }
            else
            {
                transform.localScale = originalScale;
            }
        }
#endif

        private void Awake()
        {
            if (originalScale == Vector3.zero)
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

        public void ScaleBack()
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
                    transform.LeanScale(originalScale, duration);
                }
            }
            else
            {
                scaled = false;
                transform.localScale = originalScale;
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
                    transform.LeanScale(originalScale, duration);
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