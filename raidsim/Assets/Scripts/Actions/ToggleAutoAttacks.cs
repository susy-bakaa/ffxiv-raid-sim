using UnityEngine;
using UnityEngine.Events;
using TMPro;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Actions
{
    public class ToggleAutoAttacks : MonoBehaviour
    {
        [SerializeField] private ActionController actionController;
        [SerializeField] private TextMeshProUGUI statusText;
        public UnityEvent<bool> onToggle;

        bool state = false;

        private void Start()
        {
            if (actionController != null)
                state = actionController.autoAttackEnabled;
            if (statusText != null)
            {
                statusText.text = this.state ? "<color=green>ON</color>" : "<color=red>OFF</color>";
            }
        }

        private void Update()
        {
            if (actionController == null)
                return;

            if (Utilities.RateLimiter(63))
            {
                state = actionController.autoAttackEnabled;
                if (statusText != null)
                {
                    statusText.text = this.state ? "<color=green>ON</color>" : "<color=red>OFF</color>";
                }
            }
        }

        public void ToggleStatus()
        {
            state = !state;
            ToggleStatus(state);
        }

        public void ToggleStatus(bool state)
        {
            if (actionController == null)
                return;

            this.state = state;

            actionController.autoAttackEnabled = this.state;
            if (statusText != null)
            {
                statusText.text = this.state ? "<color=green>ON</color>" : "<color=red>OFF</color>";
            }
            onToggle.Invoke(this.state);
        }
    }
}