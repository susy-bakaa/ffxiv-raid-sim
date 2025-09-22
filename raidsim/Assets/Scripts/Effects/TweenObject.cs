// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.Visuals
{
    public class TweenObject : MonoBehaviour
    {
        GameObject m_object;

        public LeanTweenType tweenType = LeanTweenType.linear;
        public float tweenDuration;
        public bool onStart;
        public Vector3 tweenScale = Vector3.zero;

        public UnityEvent onFinish;

        private void Awake()
        {
            m_object = gameObject;
        }

        private void Start()
        {
            if (onStart)
            {
                if (tweenScale != Vector3.zero)
                {
                    LeanTween.scale(m_object, tweenScale, tweenDuration).setEase(tweenType).setOnComplete(() => onFinish.Invoke());
                }
            }
        }

        private void Update()
        {

        }

        public void TriggerTween(float duration = -1)
        {
            if (duration < 0)
            {
                duration = tweenDuration;
            }

            if (tweenScale != Vector3.zero)
            {
                LeanTween.scale(m_object, tweenScale, duration).setEase(tweenType).setOnComplete(() => onFinish.Invoke());
            }
        }
    }
}