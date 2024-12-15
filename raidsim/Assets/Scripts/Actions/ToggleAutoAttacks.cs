using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToggleAutoAttacks : MonoBehaviour
{
    [SerializeField] private ActionController actionController;
    [SerializeField] private TextMeshProUGUI statusText;

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

    void Update()
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
    }
}
