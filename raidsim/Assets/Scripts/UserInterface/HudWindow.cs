using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.UI
{
    public class HudWindow : MonoBehaviour
    {
        protected CanvasGroup group;

        [Header("Base Hud Window")]
        public bool isOpen = false;

        public UnityEvent onOpen;
        public UnityEvent onClose;

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }
    }
}