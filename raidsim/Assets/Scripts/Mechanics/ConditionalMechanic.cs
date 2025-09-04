using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.Mechanics
{
    public class ConditionalMechanic : FightMechanic
    {
        public enum ComparisonType { boolean, integer, floatingPoint }
        public enum ComparisonOperator { equalTo, notEqualTo, greaterThan, lessThan, greaterThanOrEqualTo, lessThanOrEqualTo }

        public bool ifTrue = true;
        public ComparisonType comparisonType = ComparisonType.boolean;
        [ShowIf(nameof(comparisonType), ComparisonType.integer)] public int intValue;
        [ShowIf(nameof(comparisonType), ComparisonType.floatingPoint)] public float floatValue;
        [HideIf(nameof(comparisonType), ComparisonType.boolean)] public ComparisonOperator comparisonOperator = ComparisonOperator.equalTo;
        public UnityEvent onPass;
        public UnityEvent onFail;

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

            TriggerMechanic(conditionMet);
        }

        public void TriggerMechanic(bool value)
        {
            if (!mechanicEnabled)
                return;

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
    }
}