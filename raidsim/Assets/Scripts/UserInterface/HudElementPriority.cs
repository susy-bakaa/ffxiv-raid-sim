using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HudElementPriority : MonoBehaviour
{
    public List<HudElement> hudElements = new List<HudElement>();
    public bool autoUpdate = true;

    private int rateLimit;

    void Awake()
    {
        rateLimit = UnityEngine.Random.Range(60, 66);
    }

    void Update()
    {
        if (!autoUpdate)
            return;

        // Only once per second instead of every frame
        if (Utilities.RateLimiter(rateLimit))
        {
            UpdateSorting();
        }
    }

    public void UpdateSorting()
    {
        hudElements.Clear();
        hudElements.AddRange(GetComponentsInChildren<HudElement>(true));

        // Sort hudElements based on priority
        var sortedElements = hudElements.OrderBy(element => element.priority).ToList();

        // Update sibling indices
        for (int i = 0; i < sortedElements.Count; i++)
        {
            sortedElements[i].transform.SetSiblingIndex(i);
        }
    }
}
