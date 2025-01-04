using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static StatusEffectData;

public class RandomMechanicSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public int[] results;
    public TriggerRandomMechanic target;

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
        if (target != null && results != null && results.Length > 0)
        {
            int maxLength = results.Length - 1;
            if (value > maxLength)
            {
                value = maxLength;
            }
            if (value < 0)
            {
                value = 0;
            }

            target.editorForcedRandomEventResult = results[value];
        }
        else
        {
            Debug.LogWarning($"RandomMechanicSelector {gameObject.name} component is missing a valid target or results!");
        }
    }
}
