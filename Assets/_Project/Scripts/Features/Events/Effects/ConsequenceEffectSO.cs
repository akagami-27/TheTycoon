using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Base ScriptableObject for consequence effects applied upon selecting an event choice.
    /// Implements IEffect to modify simulation state via ISimulationContext.
    /// </summary>
    public abstract class ConsequenceEffectSO : ScriptableObject, IEffect
    {
        public abstract string Description { get; }
        public abstract void Apply(ISimulationContext context);
    }
}
