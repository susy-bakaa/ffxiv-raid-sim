using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Mechanics;

namespace dev.susybaka.raidsim.UI
{
    public class RandomMechanicSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;

        public int[] results;
        public TriggerRandomMechanic target;
        public UnityEvent<int> onSelect;
        public bool log = true;

        private void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            Select(0);
        }

        private void Update()
        {
            dropdown.interactable = !FightTimeline.Instance.playing;
        }

        public void Select(int value)
        {
            if (target == null)
            {
                if (log)
                    Debug.LogWarning($"RandomMechanicSelector {gameObject.name} component is missing a valid target or results! This might be unintended.");
            }

            if (results != null && results.Length > 0)
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

                if (target != null)
                    target.editorForcedRandomEventResult = results[value];

                onSelect.Invoke(results[value]);
            }
        }
    }
}