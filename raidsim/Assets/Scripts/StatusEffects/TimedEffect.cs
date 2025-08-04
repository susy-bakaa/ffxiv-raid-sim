using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.Core;
using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.StatusEffects
{
    public class TimedEffect : StatusEffect
    {
        [Header("Timed Effect Settings")]
        [Tooltip("Duration in seconds before the timer triggers the effect.")]
        public float timer = 5f;
        [Tooltip("Event invoked when the timer reaches zero.")]
        public UnityEvent onTimerReachZero;

        private float currentTime;

        public override void OnUpdate(CharacterState state)
        {
            base.OnUpdate(state);

            currentTime -= FightTimeline.deltaTime;

            if (currentTime <= 0f)
            {
                onTimerReachZero.Invoke();
                currentTime = timer;
            }
        }
    }
}