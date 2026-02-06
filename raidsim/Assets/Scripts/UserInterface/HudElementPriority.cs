// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    public class HudElementPriority : MonoBehaviour
    {
        public List<HudElement> hudElements = new List<HudElement>();
        public bool autoUpdate = true;
        public bool fetchOnStart = false;
        public bool restrictToTopLevel = false;

        private int childCount = 0;
        private bool updating = false;

        private void Start()
        {
            if (fetchOnStart)
            {
                hudElements = GetComponentsInChildren<HudElement>(true).ToList();

                if (restrictToTopLevel)
                {
                    hudElements = hudElements.Where(element => element.transform.parent == transform).ToList();
                }
            }
        }

        private void Update()
        {
            if (!autoUpdate)
                return;

            // Only once per second instead of every frame
            if (transform.childCount != childCount && !updating)
            {
                updating = true;
                UpdateSorting();
            }
        }

        public void UpdateSorting()
        {
            if (transform.childCount == 0)
                return;

            if (transform.childCount != childCount)
            {
                hudElements.Clear();
                HudElement[] childElements = GetComponentsInChildren<HudElement>(true);
                foreach (HudElement childElement in childElements)
                {
                    if (restrictToTopLevel && childElement.transform.parent != transform)
                        continue;

                    if (childElement.transform != transform)
                        hudElements.Add(childElement);
                }
                childCount = transform.childCount;
            }

            // Separate elements into three groups
            var sortedElements = hudElements.Where(element => !element.omitSorting && !element.hidden && (element.CanvasGroup == null || element.CanvasGroup.alpha > 0.1f))
                                            .OrderBy(element => element.priority)
                                            .ToList();
            
            var hiddenButSortedElements = hudElements.Where(element => !element.omitSorting && (element.hidden || (element.CanvasGroup != null && element.CanvasGroup.alpha < 0.1f)))
                                                     .OrderBy(element => element.priority)
                                                     .ToList();
            
            var omittedElements = hudElements.Where(element => element.omitSorting).ToList();

            int currentIndex = 0;

            if (hudElements == null || hudElements.Count < 1)
                return;

            // Set sibling indices for visible sorted elements
            for (int i = 0; i < sortedElements.Count; i++)
            {
                // Skip if currentIndex exceeds hudElements count (safety check)
                if (hudElements.Count <= currentIndex)
                    continue;

                if (sortedElements[i] == null)
                    continue;

                sortedElements[i].transform.SetSiblingIndex(currentIndex++);
            }

            // Set sibling indices for hidden but sorted elements
            for (int i = 0; i < hiddenButSortedElements.Count; i++)
            {
                // Skip if currentIndex exceeds hudElements count (safety check)
                if (hudElements.Count <= currentIndex)
                    continue;

                if (hiddenButSortedElements[i] == null)
                    continue;

                hiddenButSortedElements[i].transform.SetSiblingIndex(currentIndex++);
            }

            // Set sibling indices for omitted elements (keep their relative order)
            for (int i = 0; i < omittedElements.Count; i++)
            {
                // Skip if currentIndex exceeds hudElements count (safety check)
                if (hudElements.Count <= currentIndex)
                    continue;

                if (omittedElements[i] == null)
                    continue;

                omittedElements[i].transform.SetSiblingIndex(currentIndex++);
            }

            updating = false;
        }
    }
}