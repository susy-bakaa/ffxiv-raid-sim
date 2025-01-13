using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomFightTimelineResultSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public int[] results;
    public int id;

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
        if (id > -1 && results != null && results.Length > 0)
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

            if (FightTimeline.Instance != null)
            {
                if (results[value] > -1)
                {
                    Debug.Log("Set");
                    FightTimeline.Instance.SetRandomEventResult(id, results[value]);
                }
                else
                {
                    Debug.Log("Clear");
                    FightTimeline.Instance.ClearRandomEventResult(id);
                }
            }
            else
            {
                Debug.LogWarning($"RandomFightTimelineResultSelector {gameObject.name} component is missing a valid FightTimeline instance!");
            }
        }
        else
        {
            Debug.LogWarning($"RandomFightTimelineResultSelector {gameObject.name} component is missing a valid target or results!");
        }
    }
}
