using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.raidsim.Inputs;

namespace dev.susybaka.raidsim.UI
{
    public class SetTypingHelper : MonoBehaviour
    {
        [SerializeField] private UserInput input;
        [SerializeField] private string identifier = string.Empty;

        private void Awake()
        {
            input = FightTimeline.Instance.input;
        }

        public void SetTyping(bool value)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                Debug.LogWarning("Identifier is null or empty. Cannot set typing state.");
                return;
            }

            input.SetTypingState(identifier, value);
        }
    }
}