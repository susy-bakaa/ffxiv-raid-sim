// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.UI
{
    public class RandomFightTimelineResultSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;

        public bool log = false;
        public int[] results;
        public int id;
        public UnityEvent<int> onResultSelected;

        private bool setup = false;
        private int selectedValue = 0;
        private int componentId = 0;

        private void Start()
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

        private void Update()
        {
            dropdown.interactable = !FightTimeline.Instance.playing;
        }

        private void OnEnable()
        {
            if (!setup)
                return;

            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onPlay.AddListener(SetValue);
            }
        }

        private void OnDisable()
        {
            if (FightTimeline.Instance != null)
            {
                FightTimeline.Instance.onPlay.RemoveListener(SetValue);
            }
        }

        private void OnDestroy()
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
                onResultSelected?.Invoke(results[selectedValue]);
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
}