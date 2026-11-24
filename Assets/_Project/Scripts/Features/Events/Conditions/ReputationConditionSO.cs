using System;
using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Prerequisite condition requiring a minimum (and optional maximum) corporate reputation percentage.
    /// </summary>
    [CreateAssetMenu(fileName = "Condition_Reputation", menuName = "The Tycoon/Conditions/Corporate Reputation")]
    public class ReputationConditionSO : PrerequisiteConditionSO
    {
        [Range(0, 100)]
        [SerializeField] private int _minimumReputation;
        [Range(0, 100)]
        [SerializeField] private int _maximumReputation = 100;

        public int MinimumReputation => _minimumReputation;
        public int MaximumReputation => _maximumReputation;

        public void SetReputationRange(int minimum, int maximum = 100)
        {
            if (minimum < 0 || minimum > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(minimum), "Reputation must be between 0 and 100.");
            }
            if (maximum < minimum || maximum > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum reputation must be >= minimum and <= 100.");
            }

            _minimumReputation = minimum;
            _maximumReputation = maximum;
        }

        public override bool Evaluate(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.CorporateReputation >= _minimumReputation && context.CorporateReputation <= _maximumReputation;
        }

        public override string GetFailureReason(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.CorporateReputation < _minimumReputation)
            {
                return $"Requires at least {_minimumReputation}% Reputation (Current: {context.CorporateReputation}%)";
            }
            return $"Requires at most {_maximumReputation}% Reputation (Current: {context.CorporateReputation}%)";
        }
    }
}
