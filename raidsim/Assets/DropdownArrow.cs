using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownArrow : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public GameObject standard;
    public GameObject expanded;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    void Update()
    {
        if (Utilities.RateLimiter(27))
        {
            if (dropdown.IsExpanded)
            {
                standard.SetActive(false);
                expanded.SetActive(true);
            }
            else
            {
                expanded.SetActive(false);
                standard.SetActive(true);
            }
        }
    }
}
