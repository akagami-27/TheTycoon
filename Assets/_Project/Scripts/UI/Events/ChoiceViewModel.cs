using System;
using System.Collections.Generic;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// Presentation data model for an individual event choice option.
    /// Decouples view rendering from simulation domain entities.
    /// </summary>
    public class ChoiceViewModel
    {
        public string ChoiceId { get; }
        public string Title { get; }
        public string Description { get; }
        public bool IsEnabled { get; }
        public string RequirementHint { get; }
        public IReadOnlyList<string> ImmediateConsequences { get; }
        public IReadOnlyList<string> DelayedConsequences { get; }

        public ChoiceViewModel(
            string choiceId,
            string title,
            string description,
            bool isEnabled,
            string requirementHint,
            IReadOnlyList<string> immediateConsequences,
            IReadOnlyList<string> delayedConsequences)
        {
            ChoiceId = choiceId ?? throw new ArgumentNullException(nameof(choiceId));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? string.Empty;
            IsEnabled = isEnabled;
            RequirementHint = requirementHint ?? string.Empty;
            ImmediateConsequences = immediateConsequences ?? Array.Empty<string>();
            DelayedConsequences = delayedConsequences ?? Array.Empty<string>();
        }
    }
}
