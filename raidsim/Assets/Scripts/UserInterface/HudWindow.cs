using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.Core.GlobalData.Flag;

namespace dev.susybaka.raidsim.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HudWindow : MonoBehaviour
    {
        protected CanvasGroup group;
        public CanvasGroup Group { get { return group; } }

        [Header("Base Hud Window")]
        public bool isOpen = false;
        public Flag isInteractable = new Flag("isInteractable", new List<FlagValue> { new FlagValue("base", true) }, AggregateLogic.AllTrue);

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
            isInteractable.SetFlag("closeWindow", true);
            UpdateInteractableState();
            group.alpha = 1f;

            onOpen.Invoke();
        }

        public void CloseWindow()
        {
            if (group == null)
            {
                Awake();
            }

            isOpen = false;
            isInteractable.SetFlag("closeWindow", false);
            UpdateInteractableState();
            group.alpha = 0f;

            onClose.Invoke();
        }

        public void EnableInteractions()
        {
            isInteractable.SetFlag("enableInteractions", true);
            UpdateInteractableState();
        }

        public void DisableInteractions()
        {
            isInteractable.SetFlag("enableInteractions", false);
            UpdateInteractableState();
        }

        private void UpdateInteractableState()
        {
            if (group == null)
                return;

            if (isInteractable.value)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }
            else
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }
    }
}