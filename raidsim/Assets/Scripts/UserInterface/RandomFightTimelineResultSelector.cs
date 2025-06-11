using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RandomFightTimelineResultSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;

    public int[] results;
    public int id;
    public bool log = false;

    private bool setup = false;
    private int selectedValue = 0;
    private int componentId = 0;

    void Start()
    {
        componentId = Random.Range(0, 10000);
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        Select(0);

        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.onPlay.AddListener(SetValue);
        }

        Utilities.FunctionTimer.Create(this, () => setup = true, 1f, $"RandomFightTimelineResultSelector_{gameObject.name}_{componentId}_setup_delay", true, true);
    }

    void Update()
    {
        dropdown.interactable = !FightTimeline.Instance.playing;
    }

    void OnEnable()
    {
        if (!setup)
            return;

        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.onPlay.AddListener(SetValue);
        }
    }

    void OnDisable()
    {
        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.onPlay.RemoveListener(SetValue);
        }
    }

    void OnDestroy()
    {
        if (FightTimeline.Instance != null)
        {
            FightTimeline.Instance.onPlay.RemoveListener(SetValue);
        }
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

            selectedValue = value;
            SetValue();
        }
        else
        {
            Debug.LogWarning($"RandomFightTimelineResultSelector {gameObject.name} component is missing a valid target or results!");
        }
    }

    private void SetValue()
    {
        if (FightTimeline.Instance != null)
        {
            if (results[selectedValue] > -1)
            {
                if (log)
                    Debug.Log($"[RandomFightTimelineResultSelector] Set result for id {id} to {results[selectedValue]}");
                FightTimeline.Instance.SetRandomEventResult(id, results[selectedValue]);
            }
            else
            {
                if (log)
                    Debug.Log($"[RandomFightTimelineResultSelector] Clear results for event id {id}");
                FightTimeline.Instance.ClearRandomEventResult(id);
            }
        }
        else
        {
            Debug.LogWarning($"RandomFightTimelineResultSelector {gameObject.name} component is missing a valid FightTimeline instance!");
        }
    }
}
