using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Base ScriptableObject for prerequisite conditions that control event eligibility and choice availability.
    /// Implements ICondition to allow seamless evaluation against ISimulationContext.
    /// </summary>
    public abstract class PrerequisiteConditionSO : ScriptableObject, ICondition
    {
        public abstract bool Evaluate(ISimulationContext context);
        public abstract string GetFailureReason(ISimulationContext context);
    }
}
