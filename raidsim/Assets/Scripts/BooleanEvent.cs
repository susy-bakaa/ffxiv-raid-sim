using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.Events
{
    public class BooleanEvent : MonoBehaviour
    {
        public bool eventEnabled = true;
        public bool log = false;
        public UnityEvent onTrue;
        public UnityEvent onFalse;

        public void TriggerEvent(bool value)
        {
            if (!eventEnabled)
            {
                if (log)
                    Debug.Log("[BooleanEvent] Component is disabled, events will not be triggered.");
                return;
            }

            if (log)
                Debug.Log($"[BooleanEvent] Triggering event with value of '{value}'.");

            if (value)
            {
                onTrue.Invoke();
            }
            else
            {
                onFalse.Invoke();
            }
        }
    }
}