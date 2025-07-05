using UnityEngine;
using TMPro;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Mechanics;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.UI
{
    public class DebuffSelector : MonoBehaviour
    {
        TMP_Dropdown dropdown;

        public StatusEffectInfo[] effects;
        public RaidwideDebuffsMechanic target;

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
}