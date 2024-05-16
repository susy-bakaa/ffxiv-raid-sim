using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HudElementPriority : MonoBehaviour
{
    public List<HudElement> hudElements = new List<HudElement>();

    void Update()
    {
        // Only once per second instead of every frame
        if (Utilities.RateLimiter(60))
        {
            hudElements.Clear();
            hudElements.AddRange(GetComponentsInChildren<HudElement>());

            // Sort hudElements based on priority
            var sortedElements = hudElements.OrderBy(element => element.priority).ToList();

            // Update sibling indices
            for (int i = 0; i < sortedElements.Count; i++)
            {
                sortedElements[i].transform.SetSiblingIndex(i);
            }
        }
    }
}
