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

        // Sort hudElements based on priority
        var sortedElements = hudElements.OrderBy(element => element.priority).ToList();

        // Update sibling indices
        for (int i = 0; i < sortedElements.Count; i++)
        {
            sortedElements[i].transform.SetSiblingIndex(i);
        }
    }
}
