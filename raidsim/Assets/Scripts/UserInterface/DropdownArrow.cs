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
    public UserInput userInput;

    private bool inputSet = false;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (userInput == null)
            userInput = FindObjectOfType<UserInput>();
    }

    void Update()
    {
        if (Utilities.RateLimiter(27))
        {
            if (dropdown.IsExpanded)
            {
                standard.SetActive(false);
                expanded.SetActive(true);
                if (userInput != null)
                {
                    userInput.rotationInputEnabled = false;
                    userInput.zoomInputEnabled = false;
                }
                inputSet = false;
            }
            else
            {
                expanded.SetActive(false);
                standard.SetActive(true);
                if (userInput != null && !inputSet)
                {
                    userInput.rotationInputEnabled = true;
                    userInput.zoomInputEnabled = true;
                    inputSet = true;
                }
            }
        }
    }
}
