using System;
using System.Collections.Generic;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Manages the deck of business events, filters eligible candidates based on prerequisites,
    /// tracks unique triggers and cooldowns, and executes weighted event draws.
    /// </summary>
    public class EventDeckManager
    {
        private readonly List<EventDefinitionSO> _eventDeck = new();
        private readonly HashSet<string> _resolvedEventIds = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameDate> _eventCooldowns = new(StringComparer.OrdinalIgnoreCase);
        private readonly IRandomProvider _randomProvider;

        public EventDefinitionSO ActiveEvent { get; private set; }
        public bool HasActiveEvent => ActiveEvent != null;

        public IReadOnlyList<EventDefinitionSO> RegisteredEvents => _eventDeck;
        public IReadOnlyCollection<string> ResolvedEventIds => _resolvedEventIds;

        public bool IsEventResolved(string eventId) => !string.IsNullOrEmpty(eventId) && _resolvedEventIds.Contains(eventId);

        public EventDeckManager(IRandomProvider randomProvider = null)
        {
            _randomProvider = randomProvider ?? new SystemRandomProvider();
        }

        public void RegisterEvent(EventDefinitionSO eventDefinition)
        {
            if (eventDefinition == null)
            {
                throw new ArgumentNullException(nameof(eventDefinition));
            }
            if (string.IsNullOrWhiteSpace(eventDefinition.EventId))
            {
                throw new ArgumentException("Event definition must have a valid EventId.", nameof(eventDefinition));
            }
            if (_eventDeck.Exists(e => string.Equals(e.EventId, eventDefinition.EventId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An event with ID '{eventDefinition.EventId}' is already registered.");
            }

            _eventDeck.Add(eventDefinition);
        }

        public void RegisterEvents(IEnumerable<EventDefinitionSO> eventDefinitions)
        {
            if (eventDefinitions == null)
            {
                throw new ArgumentNullException(nameof(eventDefinitions));
            }

            foreach (var evt in eventDefinitions)
            {
                RegisterEvent(evt);
            }
        }

        public IReadOnlyList<EventDefinitionSO> GetEligibleEvents(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eligible = new List<EventDefinitionSO>();
            GameDate currentDate = context.CurrentDate;

            foreach (var evt in _eventDeck)
            {
                if (evt == null)
                {
                    continue;
                }

                // 1. Check Unique restriction
                if (evt.IsUnique && _resolvedEventIds.Contains(evt.EventId))
                {
                    continue;
                }

                // 2. Check Cooldown restriction
                if (_eventCooldowns.TryGetValue(evt.EventId, out GameDate availableDate))
                {
                    if (currentDate < availableDate)
                    {
                        continue;
                    }
                }

                // 3. Check Prerequisites
                if (!evt.ArePrerequisitesMet(context))
                {
                    continue;
                }

                eligible.Add(evt);
            }

            return eligible;
        }

        /// <summary>
        /// Selects an eligible event using weighted probability and sets it as the ActiveEvent.
        /// Returns null if no events are eligible.
        /// </summary>
        public EventDefinitionSO DrawEvent(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eligible = GetEligibleEvents(context);
            if (eligible.Count == 0)
            {
                ActiveEvent = null;
                return null;
            }

            int totalWeight = 0;
            foreach (var evt in eligible)
            {
                totalWeight += evt.Weight;
            }

            int roll = _randomProvider.Next(0, totalWeight);
            int cumulative = 0;

            foreach (var evt in eligible)
            {
                cumulative += evt.Weight;
                if (roll < cumulative)
                {
                    ActiveEvent = evt;
                    return evt;
                }
            }

            ActiveEvent = eligible[eligible.Count - 1];
            return ActiveEvent;
        }

        public void ClearActiveEvent()
        {
            ActiveEvent = null;
        }

        public void RecordEventResolved(EventDefinitionSO eventDefinition, GameDate currentDate)
        {
            if (eventDefinition == null)
            {
                throw new ArgumentNullException(nameof(eventDefinition));
            }

            _resolvedEventIds.Add(eventDefinition.EventId);

            if (eventDefinition.CooldownMonths > 0)
            {
                GameDate nextAvailableDate = currentDate.AdvanceMonths(eventDefinition.CooldownMonths);
                _eventCooldowns[eventDefinition.EventId] = nextAvailableDate;
            }

            if (ActiveEvent == eventDefinition)
            {
                ClearActiveEvent();
            }
        }
    }
}
