using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Events
{
    public class CombinedEvent : MonoBehaviour
    {
        public UnityEvent<CharacterState> combinedCharacterStateEvent;
        public UnityEvent basicCombinedEvent;
        public float delay = 0;
        public bool runOnStart = false;

        private int randomIndex;

        private void Start()
        {
            randomIndex = UnityEngine.Random.Range(1000, 10000);

            if (runOnStart)
            {
                if (delay > 0)
                {
                    Utilities.FunctionTimer.Create(this, () => BasicCombinedEvent(), delay, $"{randomIndex}_{gameObject.name}_combinedEvent_start_delay", true, true);
                }
                else
                {
                    BasicCombinedEvent();
                }
            }
        }

        public void BasicCombinedEvent()
        {
            basicCombinedEvent.Invoke();
        }

        public void CombinedCharacterStateEvent(CharacterState characterState)
        {
            combinedCharacterStateEvent.Invoke(characterState);
        }
    }
}