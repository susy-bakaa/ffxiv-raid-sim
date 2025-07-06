using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementGroup : MonoBehaviour
    {
        public bool log = false;
        public bool populateAutomatically = false;
        public List<HudElement> elements = new List<HudElement>();
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerEnter;
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerExit;
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerClick;

        private int id = -1;

        private void Start()
        {
            id = Random.Range(0, 10001);

            Utilities.FunctionTimer.Create(this, Setup, 0.5f, $"HudElementGroup_{id}_CleanElements", true, false);
        }

        private void Setup()
        {
            if (populateAutomatically)
            {
                PopulateElements();
            }

            if (elements != null && elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    elements[i].onPointerEnter.AddListener(OnPointerEnter);
                    elements[i].onPointerExit.AddListener(OnPointerExit);
                    elements[i].onPointerClick.AddListener(OnPointerClick);
                }
            }
        }

        private void PopulateElements()
        {
            elements.Clear();

            HudElement[] foundElements = GetComponentsInChildren<HudElement>(true);
            List<HudElement> filteredElements = new List<HudElement>(foundElements.Length);

            for (int i = 0; i < foundElements.Length; i++)
            {
                if (foundElements[i] != this.GetComponent<HudElement>())
                {
                    filteredElements.Add(foundElements[i]);
                }
            }

            elements.AddRange(filteredElements);
        }

        private void OnPointerEnter(HudElementEventInfo eventInfo)
        {
            if (log)
                Debug.Log($"[HudElementGroup] Pointer entered on element: {eventInfo.element.name}");

            onPointerEnter.Invoke(eventInfo);
        }

        private void OnPointerExit(HudElementEventInfo eventInfo)
        {
            if (log)
                Debug.Log($"[HudElementGroup] Pointer exited on element: {eventInfo.element.name}");

            onPointerExit.Invoke(eventInfo);
        }

        private void OnPointerClick(HudElementEventInfo eventInfo)
        {
            if (log)
                Debug.Log($"[HudElementGroup] Button was clicked on element: {eventInfo.element.name}");

            onPointerClick.Invoke(eventInfo);
        }
    }
}