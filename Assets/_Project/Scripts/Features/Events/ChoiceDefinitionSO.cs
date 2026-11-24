using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// ScriptableObject defining an individual player decision option within an event.
    /// Specifies availability conditions, immediate consequences, and scheduled delayed effects.
    /// </summary>
    [CreateAssetMenu(fileName = "Choice_New", menuName = "The Tycoon/Events/Choice Definition")]
    public class ChoiceDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _choiceId;
        [SerializeField] private string _choiceTitle;
        [TextArea(2, 4)]
        [SerializeField] private string _choiceDescription;

        [SerializeField] private PrerequisiteConditionSO[] _requirements;
        [SerializeField] private ConsequenceEffectSO[] _immediateEffects;
        [SerializeField] private DelayedEffectDefinition[] _delayedEffects;

        // Fallbacks for programmatic construction in tests
        private List<ICondition> _directRequirements;
        private List<IEffect> _directImmediateEffects;
        private List<DelayedEffectDefinition> _directDelayedEffects;

        public string ChoiceId => _choiceId;
        public string ChoiceTitle => _choiceTitle;
        public string ChoiceDescription => _choiceDescription;

        public IReadOnlyList<ICondition> Requirements
        {
            get
            {
                if (_directRequirements != null)
                {
                    return _directRequirements;
                }
                return _requirements ?? Array.Empty<ICondition>();
            }
        }

        public IReadOnlyList<IEffect> ImmediateEffects
        {
            get
            {
                if (_directImmediateEffects != null)
                {
                    return _directImmediateEffects;
                }
                return _immediateEffects ?? Array.Empty<IEffect>();
            }
        }

        public IReadOnlyList<DelayedEffectDefinition> DelayedEffects
        {
            get
            {
                if (_directDelayedEffects != null)
                {
                    return _directDelayedEffects;
                }
                return _delayedEffects ?? Array.Empty<DelayedEffectDefinition>();
            }
        }

        public void Configure(
            string choiceId,
            string choiceTitle,
            string choiceDescription = "",
            IEnumerable<ICondition> requirements = null,
            IEnumerable<IEffect> immediateEffects = null,
            IEnumerable<DelayedEffectDefinition> delayedEffects = null)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                throw new ArgumentException("Choice ID cannot be null or whitespace.", nameof(choiceId));
            }
            if (string.IsNullOrWhiteSpace(choiceTitle))
            {
                throw new ArgumentException("Choice title cannot be null or whitespace.", nameof(choiceTitle));
            }

            _choiceId = choiceId;
            _choiceTitle = choiceTitle;
            _choiceDescription = choiceDescription ?? string.Empty;

            _directRequirements = requirements != null ? new List<ICondition>(requirements) : new List<ICondition>();
            _directImmediateEffects = immediateEffects != null ? new List<IEffect>(immediateEffects) : new List<IEffect>();
            _directDelayedEffects = delayedEffects != null ? new List<DelayedEffectDefinition>(delayedEffects) : new List<DelayedEffectDefinition>();

            var reqList = new List<PrerequisiteConditionSO>();
            foreach (var req in _directRequirements)
            {
                if (req is PrerequisiteConditionSO reqSO)
                {
                    reqList.Add(reqSO);
                }
            }
            _requirements = reqList.ToArray();

            var effectList = new List<ConsequenceEffectSO>();
            foreach (var eff in _directImmediateEffects)
            {
                if (eff is ConsequenceEffectSO effSO)
                {
                    effectList.Add(effSO);
                }
            }
            _immediateEffects = effectList.ToArray();
            _delayedEffects = _directDelayedEffects.ToArray();
        }

        public bool CanSelect(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var req in Requirements)
            {
                if (req == null)
                {
                    continue;
                }
                if (!req.Evaluate(context))
                {
                    return false;
                }
            }
            return true;
        }

        public void ApplyImmediateEffects(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var effect in ImmediateEffects)
            {
                effect?.Apply(context);
            }
        }
    }
}
