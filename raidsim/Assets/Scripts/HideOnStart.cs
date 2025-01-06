using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HideOnStart : MonoBehaviour
{
    CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
