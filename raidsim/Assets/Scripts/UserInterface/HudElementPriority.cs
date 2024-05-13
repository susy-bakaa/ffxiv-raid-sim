using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HudElementPriority : MonoBehaviour
{
    public List<HudElement> hudElements = new List<HudElement>();

    void Update()
    {
        // Sort hudElements based on priority
        var sortedElements = hudElements.OrderBy(element => element.priority).ToList();

        // Update sibling indices
        for (int i = 0; i < sortedElements.Count; i++)
        {
            sortedElements[i].transform.SetSiblingIndex(i);
        }
    }
}
