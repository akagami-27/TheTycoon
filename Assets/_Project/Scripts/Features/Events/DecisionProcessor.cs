using System;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Responsible for validating and executing player choices on business events.
    /// Enforces prerequisite validation, enacts immediate consequences, schedules delayed effects,
    /// and ensures an event cannot be resolved repeatedly.
    /// </summary>
    public class DecisionProcessor
    {
        private readonly EventDeckManager _deckManager;
        private readonly DelayedEffectScheduler _scheduler;

        public EventDeckManager DeckManager => _deckManager;
        public DelayedEffectScheduler Scheduler => _scheduler;

        public DecisionProcessor(EventDeckManager deckManager, DelayedEffectScheduler scheduler)
        {
            _deckManager = deckManager ?? throw new ArgumentNullException(nameof(deckManager));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>
        /// Validates and processes a player's choice on an event.
        /// Execution flow:
        /// 1. Verifies that the event is non-null and currently active.
        /// 2. Verifies that the choice belongs to the active event.
        /// 3. Validates that choice prerequisites are fully satisfied.
        /// 4. Executes immediate consequences against ISimulationContext.
        /// 5. Registers delayed consequences with DelayedEffectScheduler.
        /// 6. Records the event as resolved in EventDeckManager.
        /// </summary>
        public DecisionResult ProcessDecision(
            EventDefinitionSO targetEvent,
            ChoiceDefinitionSO selectedChoice,
            ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // 1. Validate active event
            if (targetEvent == null)
            {
                return DecisionResult.Failure("Cannot process decision: Target event is null.");
            }

            if (_deckManager.ActiveEvent != targetEvent)
            {
                return DecisionResult.Failure(
                    $"Event '{targetEvent.EventId}' is not the currently active event in EventDeckManager.",
                    targetEvent,
                    selectedChoice);
            }

            // 2. Validate choice belongs to event
            if (selectedChoice == null)
            {
                return DecisionResult.Failure("Cannot process decision: Selected choice is null.", targetEvent);
            }

            if (!targetEvent.ContainsChoice(selectedChoice))
            {
                return DecisionResult.Failure(
                    $"Choice '{selectedChoice.ChoiceId}' does not belong to event '{targetEvent.EventId}'.",
                    targetEvent,
                    selectedChoice);
            }

            // 3. Validate choice requirements
            if (!selectedChoice.CanSelect(context))
            {
                return DecisionResult.Failure(
                    $"Requirements for choice '{selectedChoice.ChoiceId}' are not met.",
                    targetEvent,
                    selectedChoice);
            }

            // 4. Apply immediate effects
            selectedChoice.ApplyImmediateEffects(context);

            // 5. Schedule delayed effects
            foreach (var delayedDef in selectedChoice.DelayedEffects)
            {
                if (delayedDef != null && delayedDef.Effect != null)
                {
                    _scheduler.ScheduleEffect(
                        context.CurrentDate,
                        delayedDef.DelayMonths,
                        delayedDef.Effect,
                        targetEvent.EventId);
                }
            }

            // 6. Record resolution in deck manager
            _deckManager.RecordEventResolved(targetEvent, context.CurrentDate);

            return DecisionResult.Success(targetEvent, selectedChoice);
        }
    }
}
