// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleScale : MonoBehaviour
    {
        public Vector3 scale;
        public bool setScale = false;
        public bool onUpdate = true;
        [NaughtyAttributes.ShowIf(nameof(onUpdate))] public bool pingPong = false;
        public bool overrideOriginalScale = false;
        [NaughtyAttributes.ShowIf(nameof(overrideOriginalScale))] public Vector3 originalScale = Vector3.zero;
        public float duration = 0.5f;

        private bool scaled = false;
        private bool tweenDone = true;

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
            tweenDone = true;
        }

        private void Update()
        {
            if (!onUpdate)
                return;

            if (pingPong)
            {
                if (scaled && tweenDone)
                    ScaleBack();
                else if (!scaled && tweenDone)
                    Scale();
            }
            else
            {
                Scale();
            }
        }

        public void Scale()
        {
            if (scaled || (!setScale && !tweenDone))
                return;

            if (!setScale)
            {
                if (scale != Vector3.zero)
                {
                    tweenDone = false;
                    scaled = true;
                    transform.LeanScale(scale, duration).setOnComplete(() => tweenDone = true);
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
            if (!scaled || (!setScale && !tweenDone))
                return;

            if (!setScale)
            {
                if (originalScale != Vector3.zero)
                {
                    tweenDone = false;
                    scaled = false;
                    transform.LeanScale(originalScale, duration).setOnComplete(() => tweenDone = true);
                }
                else
                {
                    tweenDone = false;
                    scaled = false;
                    originalScale = new Vector3(0.01f, 0.01f, 0.01f);
                    transform.LeanScale(originalScale, duration).setOnComplete(() => tweenDone = true);
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
            if (!scaled || (!setScale && !tweenDone))
                return;

            if (!setScale)
            {
                if (originalScale != Vector3.zero)
                {
                    tweenDone = false;
                    scaled = false;
                    transform.LeanScale(originalScale, duration).setOnComplete(() => tweenDone = true);
                }
                else
                {
                    tweenDone= false;
                    scaled = false;
                    originalScale = new Vector3(0.01f, 0.01f, 0.01f);
                    transform.LeanScale(originalScale, duration).setOnComplete(() => tweenDone = true);
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