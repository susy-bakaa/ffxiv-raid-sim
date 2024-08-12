using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CombinedEvent : MonoBehaviour
{
    public UnityEvent<CharacterState> combinedCharacterStateEvent;
    public UnityEvent basicCombinedEvent;
    public float delay = 0;
    public bool runOnStart = false;

    void Start()
    {
        if (runOnStart)
        {
            if (delay > 0)
            {
                Utilities.FunctionTimer.Create(() => BasicCombinedEvent(), delay, $"{gameObject.name}_combinedEvent_start_delay", true, true);
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
