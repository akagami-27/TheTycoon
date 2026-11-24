namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Represents the outcome of processing a player's decision on a corporate event.
    /// </summary>
    public sealed class DecisionResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public EventDefinitionSO Event { get; }
        public ChoiceDefinitionSO Choice { get; }

        private DecisionResult(bool isSuccess, string errorMessage, EventDefinitionSO evt, ChoiceDefinitionSO choice)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage ?? string.Empty;
            Event = evt;
            Choice = choice;
        }

        public static DecisionResult Success(EventDefinitionSO evt, ChoiceDefinitionSO choice)
        {
            return new DecisionResult(true, null, evt, choice);
        }

        public static DecisionResult Failure(string errorMessage, EventDefinitionSO evt = null, ChoiceDefinitionSO choice = null)
        {
            return new DecisionResult(false, errorMessage, evt, choice);
        }

        public override string ToString() => IsSuccess ? $"Success ({Event?.EventId} -> {Choice?.ChoiceId})" : $"Failure: {ErrorMessage}";
    }
}
