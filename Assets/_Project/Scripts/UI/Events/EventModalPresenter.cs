using System;
using System.Collections.Generic;
using TheTycoon.Features.Events;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// Presenter coordinating the Event Modal dialog in MVP architecture.
    /// Binds active EventDefinitionSO to ChoiceViewModels, evaluates prerequisites,
    /// dispatches player decisions to DecisionProcessor, and reports resolution status.
    /// </summary>
    public class EventModalPresenter : IDisposable
    {
        private readonly ISimulationContext _context;
        private readonly DecisionProcessor _decisionProcessor;
        private readonly IEventModalView _view;

        private EventDefinitionSO _currentEvent;

        public ISimulationContext Context => _context;
        public DecisionProcessor DecisionProcessor => _decisionProcessor;
        public IEventModalView View => _view;
        public EventDefinitionSO CurrentEvent => _currentEvent;

        public event Action OnEventResolved;

        public EventModalPresenter(
            ISimulationContext context,
            DecisionProcessor decisionProcessor,
            IEventModalView view)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _decisionProcessor = decisionProcessor ?? throw new ArgumentNullException(nameof(decisionProcessor));
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _view.OnChoiceSelected += HandleChoiceSelected;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnChoiceSelected -= HandleChoiceSelected;
            }
        }

        /// <summary>
        /// Presents the currently active event from EventDeckManager if one is pending.
        /// </summary>
        public bool PresentActiveEvent()
        {
            if (_decisionProcessor.DeckManager == null || !_decisionProcessor.DeckManager.HasActiveEvent)
            {
                _view.Hide();
                return false;
            }

            PresentEvent(_decisionProcessor.DeckManager.ActiveEvent);
            return true;
        }

        /// <summary>
        /// Formats and presents the specified EventDefinitionSO to the modal view.
        /// </summary>
        public void PresentEvent(EventDefinitionSO eventDef)
        {
            if (eventDef == null)
            {
                throw new ArgumentNullException(nameof(eventDef));
            }

            _currentEvent = eventDef;

            var choiceViewModels = new List<ChoiceViewModel>();
            foreach (var choice in eventDef.Choices)
            {
                if (choice == null)
                {
                    continue;
                }

                bool isEligible = choice.CanSelect(_context);
                string failureHint = string.Empty;

                if (!isEligible)
                {
                    foreach (var req in choice.Requirements)
                    {
                        if (req != null && !req.Evaluate(_context))
                        {
                            failureHint = req.GetFailureReason(_context);
                            break;
                        }
                    }
                }

                var immediateList = new List<string>();
                foreach (var effect in choice.ImmediateEffects)
                {
                    if (effect != null && !string.IsNullOrWhiteSpace(effect.Description))
                    {
                        immediateList.Add(effect.Description);
                    }
                }

                var delayedList = new List<string>();
                foreach (var delayed in choice.DelayedEffects)
                {
                    if (delayed?.Effect != null && !string.IsNullOrWhiteSpace(delayed.Effect.Description))
                    {
                        string monthWord = delayed.DelayMonths == 1 ? "month" : "months";
                        delayedList.Add($"In {delayed.DelayMonths} {monthWord}: {delayed.Effect.Description}");
                    }
                }

                choiceViewModels.Add(new ChoiceViewModel(
                    choice.ChoiceId,
                    choice.ChoiceTitle,
                    choice.ChoiceDescription,
                    isEligible,
                    failureHint,
                    immediateList,
                    delayedList));
            }

            _view.ShowEvent(eventDef, choiceViewModels);
        }

        public void HandleChoiceSelected(string choiceId)
        {
            if (_currentEvent == null)
            {
                return;
            }

            ChoiceDefinitionSO selectedChoice = null;
            foreach (var choice in _currentEvent.Choices)
            {
                if (choice != null && choice.ChoiceId == choiceId)
                {
                    selectedChoice = choice;
                    break;
                }
            }

            if (selectedChoice == null)
            {
                _view.SetStatusMessage($"Invalid choice '{choiceId}'.", isError: true);
                return;
            }

            _view.SetChoicesInteractable(false);

            DecisionResult result = _decisionProcessor.ProcessDecision(_currentEvent, selectedChoice, _context);
            if (result.IsSuccess)
            {
                _currentEvent = null;
                _view.Hide();
                OnEventResolved?.Invoke();
            }
            else
            {
                _view.SetStatusMessage(result.ErrorMessage, isError: true);
                _view.SetChoicesInteractable(true);
            }
        }
    }
}
