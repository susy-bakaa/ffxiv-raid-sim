// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using dev.susybaka.raidsim.Characters;
using dev.susybaka.raidsim.StatusEffects;
using dev.susybaka.raidsim.Visuals;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;

namespace dev.susybaka.raidsim.Mechanics
{
    public class CountdownMechanic : FightMechanic
    {
        [Header("Countdown Settings")]
        public string visualEffectName = "VisualEffects/Countdown_Effect";
        public bool useStatusEffect = false;
        public bool applyEffect = false;
        [ShowIf("applyEffect")] public StatusEffectInfo effect;
        [ShowIf("applyEffect")] public float applyOffset = 0f;
        public UnityEvent<ActionInfo> onFinish;
        public float finishOffset = 0f;

        private CountdownEffect countdownEffect;
        private StatusEffect statusEffect;
        private CharacterState lastCharacter;
        private bool effectApplied = false;
        private bool finished = false;

        private void Awake()
        {
            if (useStatusEffect)
            {
                statusEffect = GetComponent<StatusEffect>();
            }
        }

        private void Update()
        {
            if (useStatusEffect && statusEffect != null)
            {
                if (lastCharacter != null)
                {
                    if (statusEffect.duration <= applyOffset && !effectApplied)
                    {
                        lastCharacter.AddEffect(effect.data, lastCharacter, false, effect.tag, effect.stacks);
                        effectApplied = true;
                    }
                }
                if (finishOffset > 0f)
                {
                    if (statusEffect.duration <= finishOffset && !finished)
                    {
                        FinishCountdown(new ActionInfo(null, lastCharacter, null));
                        finished = true;
                    }
                }
                if (countdownEffect != null && statusEffect.duration > 0)
                {
                    countdownEffect.SetTexture(statusEffect.duration);
                }
                else if (countdownEffect != null && statusEffect.duration <= 0)
                {
                    countdownEffect.SetTexture(-1);
                }
            }
        }

        public void FinishCountdown(ActionInfo actionInfo)
        {
            onFinish.Invoke(actionInfo);
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (actionInfo.target != null)
                lastCharacter = actionInfo.target;
            else if (actionInfo.source != null)
                lastCharacter = actionInfo.source;

            if (!string.IsNullOrEmpty(visualEffectName))
                countdownEffect = lastCharacter.transform.Find(visualEffectName)?.GetComponentInChildren<CountdownEffect>();

            if (useStatusEffect && statusEffect != null)
            {
                if (finishOffset <= 0f)
                {
                    statusEffect.onCleanse.AddListener((CharacterState _) => FinishCountdown(actionInfo));
                    statusEffect.onExpire.AddListener((CharacterState _) => FinishCountdown(actionInfo));
                }
            }
        }
    }
}