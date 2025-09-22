// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementGroup : MonoBehaviour
    {
        HudElement thisElement;

        public bool log = false;
        public bool setupOnStart = true;
        public bool populateAutomatically = false;
        public bool restrictToTopLevel = false;
        public float setupDelay = 0.5f;
        public List<HudElement> elements = new List<HudElement>();
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerEnter;
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerExit;
        [Foldout("Events")]
        public UnityEvent<HudElementEventInfo> onPointerClick;

        private int id = -1;

#if UNITY_EDITOR
        [Button]
        public void SetAllAudioOnForCurrentElements()
        {
            if (elements == null || elements.Count < 1)
                return;

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].restrictsAudio = true;
                elements[i].playClickAudio = true;
                elements[i].playHoverAudio = true;
            }
        }
#endif

        private void Start()
        {
            thisElement = GetComponent<HudElement>();

            id = Random.Range(0, 10001);

            if (setupOnStart)
                Utilities.FunctionTimer.Create(this, Setup, setupDelay, $"HudElementGroup_{id}_Setup_Delay", true, false);
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
                    if (elements[i] == null)
                    {
                        elements.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        elements[i].onPointerEnter.AddListener(OnPointerEnter);
                        elements[i].onPointerExit.AddListener(OnPointerExit);
                        elements[i].onPointerClick.AddListener(OnPointerClick);
                    }
                }
            }
        }

        private void PopulateElements()
        {
            elements.Clear();

            HudElement[] foundElements = GetComponentsInChildren<HudElement>(true);
            List<HudElement> filteredElements = new List<HudElement>();

            if (log)
                Debug.Log($"[HudElementGroup] Found {foundElements.Length} child elements");

            for (int i = 0; i < foundElements.Length; i++)
            {
                if (foundElements[i] != thisElement)
                {
                    if (log)
                        Debug.Log($"[HudElementGroup] Child element '{foundElements[i].gameObject.name}' of index {i} is not attached to the same transform as group");

                    if (!restrictToTopLevel)
                    {
                        if (log)
                            Debug.Log($"[HudElementGroup] Child element '{foundElements[i].gameObject.name}' of index {i} was added!");

                        filteredElements.Add(foundElements[i]);
                    } // Kinda messy way to check if the element is a child of this group, but it works for now, maybe improve it later
                    else if (restrictToTopLevel && (foundElements[i].transform.parent == transform || foundElements[i].transform.parent.parent == transform))
                    {
                        if (log)
                            Debug.Log($"[HudElementGroup] Child element '{foundElements[i].gameObject.name}' of index {i} passed parent checks and was added!");

                        filteredElements.Add(foundElements[i]);
                    }
                }
            }

            elements.AddRange(filteredElements);
        }

        public void UpdateElements()
        {
            Setup();
        }

        public void AddElement(HudElement element)
        {
            if (element == null || elements == null)
                return;

            if (!elements.Contains(element))
            {
                element.onPointerEnter.AddListener(OnPointerEnter);
                element.onPointerExit.AddListener(OnPointerExit);
                element.onPointerClick.AddListener(OnPointerClick);
                elements.Add(element);
            }
        }

        public void RemoveElement(HudElement element)
        {
            if (elements == null || elements.Count < 1 || element == null)
                return;

            if (elements.Contains(element))
            {
                element.onPointerEnter.RemoveListener(OnPointerEnter);
                element.onPointerExit.RemoveListener(OnPointerExit);
                element.onPointerClick.RemoveListener(OnPointerClick);
                elements.Remove(element);
            }
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