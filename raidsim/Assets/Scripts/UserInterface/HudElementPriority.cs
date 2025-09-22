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

        private int childCount = 0;

        private void Start()
        {
            if (fetchOnStart)
            {
                hudElements = GetComponentsInChildren<HudElement>(true).ToList();
            }
        }

        private void Update()
        {
            if (!autoUpdate)
                return;

            // Only once per second instead of every frame
            if (transform.childCount != childCount)
            {
                UpdateSorting();
                childCount = transform.childCount;
            }
        }

        public void UpdateSorting()
        {
            hudElements.Clear();
            HudElement[] childElements = GetComponentsInChildren<HudElement>(true);
            foreach (HudElement childElement in childElements)
            {
                if (childElement.transform != transform)
                    hudElements.Add(childElement);
            }

            // Separate elements based on omitSorting flag
            var sortedElements = hudElements.Where(element => !element.omitSorting)
                                            .OrderBy(element => element.priority)
                                            .ToList();
            var omittedElements = hudElements.Where(element => element.omitSorting).ToList();

            if (sortedElements != null && sortedElements.Count > 0)
            {
                // Update sibling indices for sorted elements
                for (int i = 0; i < sortedElements.Count; i++)
                {
                    sortedElements[i].transform.SetSiblingIndex(i);
                }
            }

            if (omittedElements != null && omittedElements.Count > 0)
            {
                // Update sibling indices for omitted elements
                for (int i = 0; i < omittedElements.Count; i++)
                {
                    omittedElements[i].transform.SetSiblingIndex(sortedElements.Count + i);
                }
            }
        }
    }
}