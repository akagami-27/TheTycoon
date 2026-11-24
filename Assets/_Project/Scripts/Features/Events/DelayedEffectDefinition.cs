using System;
using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Configuration pairing a consequence effect with a delay in calendar months.
    /// Scheduled upon decision execution and resolved when the scheduled turn arrives.
    /// </summary>
    [Serializable]
    public class DelayedEffectDefinition
    {
        [SerializeField] private ConsequenceEffectSO _effectSO;
        [Min(1)]
        [SerializeField] private int _delayMonths = 1;

        // Pure C# reference fallback for programmatic/test construction
        private readonly IEffect _directEffect;

        public IEffect Effect => (IEffect)_effectSO ?? _directEffect;
        public int DelayMonths => _delayMonths;

        public DelayedEffectDefinition(IEffect effect, int delayMonths)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }
            if (delayMonths < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(delayMonths), "Delay months must be at least 1.");
            }

            if (effect is ConsequenceEffectSO so)
            {
                _effectSO = so;
            }
            else
            {
                _directEffect = effect;
            }
            _delayMonths = delayMonths;
        }
    }
}
