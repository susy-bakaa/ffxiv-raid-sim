using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MechanicSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public int[] options;
    public TriggerSelectedMechanic target;

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        Select(0);
    }

    void Update()
    {
        dropdown.interactable = !FightTimeline.Instance.playing;
    }

    public void Select(int value)
    {
        if (target != null && options != null && options.Length > 0)
        {
            int maxLength = options.Length - 1;
            if (value > maxLength)
            {
                value = maxLength;
            }
            if (value < 0)
            {
                value = 0;
            }

            target.SelectMechanic(options[value]);
        }
        else
        {
            Debug.LogWarning($"MechanicSelector {gameObject.name} component is missing a valid target or options!");
        }
    }
}
