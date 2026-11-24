using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// ScriptableObject defining a corporate dilemma or narrative business event.
    /// Contains narrative copy, category, prerequisites, selection weighting, and player choices.
    /// </summary>
    [CreateAssetMenu(fileName = "Event_New", menuName = "The Tycoon/Events/Event Definition")]
    public class EventDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _eventId;
        [SerializeField] private string _title;
        [TextArea(3, 6)]
        [SerializeField] private string _description;
        [SerializeField] private EventCategory _category = EventCategory.Corporate;
        [Range(1, 1000)]
        [SerializeField] private int _weight = 100;

        [SerializeField] private bool _isUnique;
        [Min(0)]
        [SerializeField] private int _cooldownMonths = 0;

        [SerializeField] private PrerequisiteConditionSO[] _prerequisites;
        [SerializeField] private ChoiceDefinitionSO[] _choices;

        // Fallbacks for programmatic construction in tests
        private List<ICondition> _directPrerequisites;
        private List<ChoiceDefinitionSO> _directChoices;

        public string EventId => _eventId;
        public string Title => _title;
        public string Description => _description;
        public EventCategory Category => _category;
        public int Weight => _weight;
        public bool IsUnique => _isUnique;
        public int CooldownMonths => _cooldownMonths;

        public IReadOnlyList<ICondition> Prerequisites
        {
            get
            {
                if (_directPrerequisites != null)
                {
                    return _directPrerequisites;
                }
                return _prerequisites ?? Array.Empty<ICondition>();
            }
        }

        public IReadOnlyList<ChoiceDefinitionSO> Choices
        {
            get
            {
                if (_directChoices != null)
                {
                    return _directChoices;
                }
                return _choices ?? Array.Empty<ChoiceDefinitionSO>();
            }
        }

        public void Configure(
            string eventId,
            string title,
            string description,
            EventCategory category = EventCategory.Corporate,
            int weight = 100,
            bool isUnique = false,
            int cooldownMonths = 0,
            IEnumerable<ICondition> prerequisites = null,
            IEnumerable<ChoiceDefinitionSO> choices = null)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException("Event ID cannot be null or whitespace.", nameof(eventId));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Event title cannot be null or whitespace.", nameof(title));
            }
            if (weight < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Event weight must be at least 1.");
            }
            if (cooldownMonths < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownMonths), "Cooldown months cannot be negative.");
            }

            _eventId = eventId;
            _title = title;
            _description = description ?? string.Empty;
            _category = category;
            _weight = weight;
            _isUnique = isUnique;
            _cooldownMonths = cooldownMonths;

            _directPrerequisites = prerequisites != null ? new List<ICondition>(prerequisites) : new List<ICondition>();
            _directChoices = choices != null ? new List<ChoiceDefinitionSO>(choices) : new List<ChoiceDefinitionSO>();

            if (_directChoices.Count == 0)
            {
                throw new ArgumentException("An event must have at least one choice.", nameof(choices));
            }

            var prereqList = new List<PrerequisiteConditionSO>();
            foreach (var p in _directPrerequisites)
            {
                if (p is PrerequisiteConditionSO pSO)
                {
                    prereqList.Add(pSO);
                }
            }
            _prerequisites = prereqList.ToArray();
            _choices = _directChoices.ToArray();
        }

        public bool ArePrerequisitesMet(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var prereq in Prerequisites)
            {
                if (prereq == null)
                {
                    continue;
                }
                if (!prereq.Evaluate(context))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsChoice(ChoiceDefinitionSO choice)
        {
            if (choice == null)
            {
                return false;
            }

            foreach (var c in Choices)
            {
                if (c == choice || string.Equals(c.ChoiceId, choice.ChoiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
