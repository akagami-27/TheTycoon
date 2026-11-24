using System;
using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Consequence effect modifying corporate reputation percentage.
    /// </summary>
    [CreateAssetMenu(fileName = "Effect_Reputation", menuName = "The Tycoon/Effects/Reputation Adjustment")]
    public class ReputationEffectSO : ConsequenceEffectSO
    {
        [SerializeField] private int _deltaReputation;

        public int DeltaReputation => _deltaReputation;

        public override string Description => _deltaReputation >= 0 
            ? $"+{_deltaReputation}% Reputation" 
            : $"{_deltaReputation}% Reputation";

        public void SetDeltaReputation(int delta)
        {
            _deltaReputation = delta;
        }

        public override void Apply(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.ModifyReputation(_deltaReputation);
        }
    }
}
