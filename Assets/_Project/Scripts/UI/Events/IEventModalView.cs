using System;
using System.Collections.Generic;
using TheTycoon.Features.Events;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// Contract for the Event Modal View component.
    /// Follows MVP architecture to allow isolated unit testing of EventModalPresenter.
    /// </summary>
    public interface IEventModalView
    {
        bool IsVisible { get; }
        event Action<string> OnChoiceSelected;

        void ShowEvent(EventDefinitionSO eventDef, IReadOnlyList<ChoiceViewModel> choices);
        void SetStatusMessage(string message, bool isError);
        void SetChoicesInteractable(bool interactable);
        void Hide();
    }
}
