using System;
using UnityEngine;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Prerequisite condition requiring a minimum company cash reserve.
    /// </summary>
    [CreateAssetMenu(fileName = "Condition_MinCash", menuName = "The Tycoon/Conditions/Minimum Cash")]
    public class MinimumCashConditionSO : PrerequisiteConditionSO
    {
        [SerializeField] private long _requiredCents;

        public Money RequiredCash => new(_requiredCents);

        public void SetRequiredCash(Money cash)
        {
            _requiredCents = cash.Cents;
        }

        public override bool Evaluate(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.CurrentCash >= RequiredCash;
        }

        public override string GetFailureReason(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return $"Requires at least {RequiredCash.ToFormattedString()} (Current: {context.CurrentCash.ToFormattedString()})";
        }
    }
}
