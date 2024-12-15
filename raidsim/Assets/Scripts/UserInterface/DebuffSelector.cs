using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static StatusEffectData;

public class DebuffSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public StatusEffectInfo[] effects;
    public RaidwideDebuffsMechanic target;

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
        if (target != null && effects != null && effects.Length > 0)
        {
            int maxLength = effects.Length - 1;
            if (value > maxLength)
            {
                value = maxLength;
            }
            if (value < 0)
            {
                value = 0;
            }

            target.playerEffect = effects[value];
        }
        else
        {
            Debug.LogWarning($"DebuffSelector {gameObject.name} component is missing a valid target or effects!");
        }
    }
}
