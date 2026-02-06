// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using static dev.susybaka.raidsim.Core.GlobalData;
using static dev.susybaka.raidsim.StatusEffects.StatusEffectData;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ConditionalMechanic : FightMechanic
    {
        public enum ComparisonType { boolean, integer, floatingPoint, statusEffect }
        public enum ComparisonOperator { equalTo, notEqualTo, greaterThan, lessThan, greaterThanOrEqualTo, lessThanOrEqualTo }
        public enum StatusEffectCondition { hasAnyEffect, doesNotHaveAnyEffect, hasAllEffects }

        public bool ifTrue = true;
        public ComparisonType comparisonType = ComparisonType.boolean;
        [ShowIf(nameof(comparisonType), ComparisonType.integer)] public int intValue;
        [ShowIf(nameof(comparisonType), ComparisonType.floatingPoint)] public float floatValue;
        [HideIf(nameof(_hideComparisonOperator))] public ComparisonOperator comparisonOperator = ComparisonOperator.equalTo;
        [ShowIf(nameof(comparisonType), ComparisonType.statusEffect)] public StatusEffectCondition statusEffectCondition = StatusEffectCondition.hasAnyEffect;
        [ShowIf(nameof(comparisonType), ComparisonType.statusEffect)] public List<StatusEffectInfo> effects;
        public UnityEvent onPass;
        public UnityEvent onFail;

        private bool _hideComparisonOperator => (comparisonType == ComparisonType.boolean || comparisonType == ComparisonType.statusEffect);

        public void TriggerMechanic(int value)
        {
            if (!mechanicEnabled)
                return;

            if (comparisonType != ComparisonType.integer)
                return;

            bool conditionMet = comparisonOperator switch
            {
                ComparisonOperator.equalTo => value == intValue,
                ComparisonOperator.notEqualTo => value != intValue,
                ComparisonOperator.greaterThan => value > intValue,
                ComparisonOperator.lessThan => value < intValue,
                ComparisonOperator.greaterThanOrEqualTo => value >= intValue,
                ComparisonOperator.lessThanOrEqualTo => value <= intValue,
                _ => false,
            };

            if (log)
                Debug.Log($"[ConditionalMechanic ({gameObject.name})] Integer comparison: {value} {comparisonOperator} {intValue} => {conditionMet}");

            TriggerMechanic(conditionMet);
        }

        public void TriggerMechanic(float value)
        {
            if (!mechanicEnabled)
                return;

            if (comparisonType != ComparisonType.floatingPoint)
                return;

            bool conditionMet = comparisonOperator switch
            {
                ComparisonOperator.equalTo => Mathf.Approximately(value, floatValue),
                ComparisonOperator.notEqualTo => !Mathf.Approximately(value, floatValue),
                ComparisonOperator.greaterThan => value > floatValue,
                ComparisonOperator.lessThan => value < floatValue,
                ComparisonOperator.greaterThanOrEqualTo => value >= floatValue,
                ComparisonOperator.lessThanOrEqualTo => value <= floatValue,
                _ => false,
            };

            if (log)
                Debug.Log($"[ConditionalMechanic ({gameObject.name})] Float comparison: {value} {comparisonOperator} {floatValue} => {conditionMet}");

            TriggerMechanic(conditionMet);
        }

        public void TriggerMechanic(bool value)
        {
            if (!mechanicEnabled)
                return;

            if (log)
               Debug.Log($"[ConditionalMechanic ({gameObject.name})] '{(!string.IsNullOrEmpty(mechanicName) ? mechanicName : "Unknown Mechanic")}' Triggered.\nBoolean comparison: {value} == {ifTrue} => {(value == ifTrue)}");

            if (value && ifTrue)
            {
                onPass?.Invoke();
            }
            else if (!value && !ifTrue)
            {
                onPass?.Invoke();
            }
            else
            {
                onFail?.Invoke();
            }
        }

        public override void TriggerMechanic(ActionInfo actionInfo)
        {
            if (!CanTrigger(actionInfo))
                return;

            if (comparisonType != ComparisonType.statusEffect)
                return;

            CharacterState character = actionInfo.target != null ? actionInfo.target : actionInfo.source;

            bool conditionMet = statusEffectCondition switch
            {
                StatusEffectCondition.hasAnyEffect => effects.Any(effect => character.HasEffect(effect.name, effect.tag)),
                StatusEffectCondition.doesNotHaveAnyEffect => !effects.Any(effect => character.HasEffect(effect.name, effect.tag)),
                StatusEffectCondition.hasAllEffects => effects.All(effect => character.HasEffect(effect.name, effect.tag)),
                _ => false,
            };

            TriggerMechanic(conditionMet);
        }
    }
}