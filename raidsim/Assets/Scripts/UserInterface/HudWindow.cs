using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HudWindow : MonoBehaviour
    {
        protected CanvasGroup group;
        public CanvasGroup Group { get { return group; } }

        [Header("Base Hud Window")]
        public bool isOpen = false;

        [Foldout("Events")]
        public UnityEvent onOpen;
        [Foldout("Events")]
        public UnityEvent onClose;

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        public void OpenWindow()
        {
            if (group == null)
            {
                Awake();
            }

            isOpen = true;

            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;

            onOpen.Invoke();
        }

        public void CloseWindow()
        {
            if (group == null)
            {
                Awake();
            }

            isOpen = false;

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            onClose.Invoke();
        }
    }
}