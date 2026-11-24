using System;
using UnityEngine;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Consequence effect modifying corporate cash via the authoritative simulation context.
    /// </summary>
    [CreateAssetMenu(fileName = "Effect_Cash", menuName = "The Tycoon/Effects/Cash Adjustment")]
    public class CashEffectSO : ConsequenceEffectSO
    {
        [SerializeField] private long _deltaCents;

        public Money Delta => new(_deltaCents);

        public override string Description => Delta.IsPositive 
            ? $"+{Delta.ToFormattedString()} Cash" 
            : $"{Delta.ToFormattedString()} Cash";

        public void SetDelta(Money delta)
        {
            _deltaCents = delta.Cents;
        }

        public override void Apply(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.ModifyCash(Delta);
        }
    }
}
