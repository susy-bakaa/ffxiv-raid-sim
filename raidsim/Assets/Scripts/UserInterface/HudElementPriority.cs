using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HudElementPriority : MonoBehaviour
{
    public List<HudElement> hudElements = new List<HudElement>();
    public bool autoUpdate = true;

    private int childCount = 0;

    void Update()
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
