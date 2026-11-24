using System;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// Contract for an individual event choice button view component.
    /// </summary>
    public interface IChoiceButtonView
    {
        void Setup(ChoiceViewModel model, Action<string> onSelected);
        void SetInteractable(bool interactable);
    }
}
