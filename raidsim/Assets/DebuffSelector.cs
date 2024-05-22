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
        target.playerEffect = effects[value];
    }
}
