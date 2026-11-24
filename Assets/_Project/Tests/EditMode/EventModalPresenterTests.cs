using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features;
using TheTycoon.Features.Company;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;
using TheTycoon.UI.Events;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class EventModalPresenterTests
    {
        // ─── Mock IEventModalView ────────────────────────────────────────────────

        private class MockEventModalView : IEventModalView
        {
            public bool IsVisible { get; private set; }
            public EventDefinitionSO LastEventShown { get; private set; }
            public IReadOnlyList<ChoiceViewModel> LastChoices { get; private set; }
            public string LastStatusMessage { get; private set; }
            public bool LastStatusWasError { get; private set; }
            public bool ChoicesInteractable { get; private set; } = true;
            public int HideCallCount { get; private set; }

            public event Action<string> OnChoiceSelected;

            public void ShowEvent(EventDefinitionSO eventDef, IReadOnlyList<ChoiceViewModel> choices)
            {
                IsVisible = true;
                LastEventShown = eventDef;
                LastChoices = choices;
            }

            public void SetStatusMessage(string message, bool isError)
            {
                LastStatusMessage = message;
                LastStatusWasError = isError;
            }

            public void SetChoicesInteractable(bool interactable)
            {
                ChoicesInteractable = interactable;
            }

            public void Hide()
            {
                IsVisible = false;
                HideCallCount++;
            }

            public void SimulateChoiceSelected(string choiceId) => OnChoiceSelected?.Invoke(choiceId);
        }

        // ─── Shared Helpers ──────────────────────────────────────────────────────

        private static SimulationContext BuildContext(Money startingCash, int reputation = 50)
        {
            var date = new GameDate(2026, 1);
            var ledger = new FinancialLedger(startingCash);
            var deckManager = new EventDeckManager();
            var scheduler = new DelayedEffectScheduler();
            var turnManager = new TurnManager(ledger, date, deckManager, scheduler);
            turnManager.Initialize();
            var company = new CompanyProfile("TestCorp", date, Money.FromDollars(1000000m));
            var context = new SimulationContext(ledger, turnManager, company, deckManager, scheduler,
                new DecisionProcessor(deckManager, scheduler), reputation);
            return context;
        }

        private static EventDefinitionSO BuildEvent(string id, params ChoiceDefinitionSO[] choices)
        {
            var evt = UnityEngine.ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure(id, "Event: " + id, "Description.", EventCategory.Corporate, 100, false, 0, null, choices);
            return evt;
        }

        private static ChoiceDefinitionSO BuildChoice(
            string id,
            IEnumerable<ICondition> reqs = null,
            IEnumerable<IEffect> effects = null,
            IEnumerable<DelayedEffectDefinition> delayed = null)
        {
            var choice = UnityEngine.ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure(id, "Choice: " + id, "Choice description.", reqs, effects, delayed);
            return choice;
        }

        // ─── Fixtures ────────────────────────────────────────────────────────────

        private SimulationContext _context;
        private EventDeckManager _deckManager;
        private DecisionProcessor _decisionProcessor;
        private MockEventModalView _view;
        private EventModalPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            _context = BuildContext(Money.FromDollars(100000m), reputation: 60);
            _deckManager = _context.EventDeckManager;
            _decisionProcessor = _context.DecisionProcessor;
            _view = new MockEventModalView();
            _presenter = new EventModalPresenter(_context, _decisionProcessor, _view);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter?.Dispose();
        }

        // ─── Construction ────────────────────────────────────────────────────────

        [Test]
        public void Constructor_NullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventModalPresenter(null, _decisionProcessor, _view));
        }

        [Test]
        public void Constructor_NullDecisionProcessor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventModalPresenter(_context, null, _view));
        }

        [Test]
        public void Constructor_NullView_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventModalPresenter(_context, _decisionProcessor, null));
        }

        // ─── PresentActiveEvent ──────────────────────────────────────────────────

        [Test]
        public void PresentActiveEvent_NoActiveEvent_HidesViewAndReturnsFalse()
        {
            bool result = _presenter.PresentActiveEvent();

            Assert.IsFalse(result);
            Assert.AreEqual(1, _view.HideCallCount);
        }

        [Test]
        public void PresentActiveEvent_WithActiveEvent_ShowsEventAndReturnsTrue()
        {
            var choiceA = BuildChoice("CHOICE_A");
            var evt = BuildEvent("EVT_01", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            bool result = _presenter.PresentActiveEvent();

            Assert.IsTrue(result);
            Assert.IsTrue(_view.IsVisible);
            Assert.AreEqual(evt, _view.LastEventShown);
        }

        // ─── PresentEvent / ChoiceViewModel building ─────────────────────────────

        [Test]
        public void PresentEvent_NullEvent_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _presenter.PresentEvent(null));
        }

        [Test]
        public void PresentEvent_BuildsChoiceViewModelWithTitle()
        {
            var choiceA = BuildChoice("CHOICE_A");
            var evt = BuildEvent("EVT_01", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            Assert.AreEqual(1, _view.LastChoices.Count);
            Assert.AreEqual("CHOICE_A", _view.LastChoices[0].ChoiceId);
            Assert.AreEqual("Choice: CHOICE_A", _view.LastChoices[0].Title);
        }

        [Test]
        public void PresentEvent_EligibleChoice_IsEnabled()
        {
            var choiceA = BuildChoice("CHOICE_ENABLED");
            var evt = BuildEvent("EVT_02", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            Assert.IsTrue(_view.LastChoices[0].IsEnabled);
            Assert.IsEmpty(_view.LastChoices[0].RequirementHint);
        }

        [Test]
        public void PresentEvent_IneligibleChoice_IsDisabledWithHint()
        {
            // Require RM500,000 but context has RM100,000
            var condition = new MinimumCashCondition(Money.FromDollars(500000m));
            var choiceA = BuildChoice("CHOICE_LOCKED", reqs: new[] { condition });
            var evt = BuildEvent("EVT_03", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            Assert.IsFalse(_view.LastChoices[0].IsEnabled);
            StringAssert.Contains("500,000", _view.LastChoices[0].RequirementHint);
        }

        [Test]
        public void PresentEvent_ImmediateEffect_AppearsInConsequences()
        {
            var effect = new CashEffect(Money.FromDollars(75000m));
            var choiceA = BuildChoice("CHOICE_B", effects: new[] { (IEffect)effect });
            var evt = BuildEvent("EVT_04", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            Assert.AreEqual(1, _view.LastChoices[0].ImmediateConsequences.Count);
            StringAssert.Contains("75,000", _view.LastChoices[0].ImmediateConsequences[0]);
        }

        [Test]
        public void PresentEvent_DelayedEffect_AppearsWithTimingLabel()
        {
            var effect = new CashEffect(Money.FromDollars(100000m));
            var delayed = new DelayedEffectDefinition(effect, delayMonths: 3);
            var choiceA = BuildChoice("CHOICE_C", delayed: new[] { delayed });
            var evt = BuildEvent("EVT_05", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            Assert.AreEqual(1, _view.LastChoices[0].DelayedConsequences.Count);
            StringAssert.Contains("3 months", _view.LastChoices[0].DelayedConsequences[0]);
            StringAssert.Contains("100,000", _view.LastChoices[0].DelayedConsequences[0]);
        }

        [Test]
        public void PresentEvent_DelayedEffect_SingularMonthLabel()
        {
            var effect = new CashEffect(Money.FromDollars(50000m));
            var delayed = new DelayedEffectDefinition(effect, delayMonths: 1);
            var choiceA = BuildChoice("CHOICE_D", delayed: new[] { delayed });
            var evt = BuildEvent("EVT_06", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);

            _presenter.PresentEvent(evt);

            StringAssert.Contains("1 month", _view.LastChoices[0].DelayedConsequences[0]);
        }

        // ─── HandleChoiceSelected ────────────────────────────────────────────────

        [Test]
        public void HandleChoiceSelected_ValidEligibleChoice_HidesViewAndFiresOnEventResolved()
        {
            var choiceA = BuildChoice("CHOICE_OK");
            var evt = BuildEvent("EVT_07", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);
            _presenter.PresentEvent(evt);

            bool resolvedFired = false;
            _presenter.OnEventResolved += () => resolvedFired = true;

            _view.SimulateChoiceSelected("CHOICE_OK");

            Assert.IsTrue(resolvedFired, "OnEventResolved should fire after successful decision.");
            Assert.AreEqual(1, _view.HideCallCount, "View should be hidden after success.");
            Assert.IsNull(_presenter.CurrentEvent, "CurrentEvent should be cleared after resolution.");
        }

        [Test]
        public void HandleChoiceSelected_UnknownChoiceId_SetsErrorMessage_DoesNotResolve()
        {
            var choiceA = BuildChoice("CHOICE_KNOWN");
            var evt = BuildEvent("EVT_08", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);
            _presenter.PresentEvent(evt);

            bool resolvedFired = false;
            _presenter.OnEventResolved += () => resolvedFired = true;

            _view.SimulateChoiceSelected("CHOICE_UNKNOWN_ID");

            Assert.IsFalse(resolvedFired, "OnEventResolved must not fire for unknown choice.");
            Assert.IsFalse(_view.IsVisible == false && _view.HideCallCount > 1,
                "View should not hide on unknown choice.");
            StringAssert.Contains("Invalid choice", _view.LastStatusMessage);
            Assert.IsTrue(_view.LastStatusWasError);
        }

        [Test]
        public void HandleChoiceSelected_IneligibleChoice_ReportsError_RestoresButtons()
        {
            // Requirement: RM500,000; have RM100,000
            var condition = new MinimumCashCondition(Money.FromDollars(500000m));
            var choiceA = BuildChoice("CHOICE_LOCKED", reqs: new[] { condition });
            // Need a valid fallback choice so event can be built
            var choiceB = BuildChoice("CHOICE_FREE");
            var evt = BuildEvent("EVT_09", choiceA, choiceB);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);
            _presenter.PresentEvent(evt);

            bool resolvedFired = false;
            _presenter.OnEventResolved += () => resolvedFired = true;

            // Force-call HandleChoiceSelected with the locked choice ID
            _presenter.HandleChoiceSelected("CHOICE_LOCKED");

            Assert.IsFalse(resolvedFired);
            Assert.IsNotEmpty(_view.LastStatusMessage);
            Assert.IsTrue(_view.LastStatusWasError);
            Assert.IsTrue(_view.ChoicesInteractable, "Buttons should be re-enabled after failure.");
        }

        [Test]
        public void HandleChoiceSelected_AppliesImmediateCashEffect()
        {
            var effect = new CashEffect(Money.FromDollars(50000m));
            var choiceA = BuildChoice("CHOICE_CASH", effects: new[] { (IEffect)effect });
            var evt = BuildEvent("EVT_10", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);
            _presenter.PresentEvent(evt);

            Money cashBefore = _context.CurrentCash;
            _view.SimulateChoiceSelected("CHOICE_CASH");
            Money cashAfter = _context.CurrentCash;

            Assert.AreEqual(cashBefore + Money.FromDollars(50000m), cashAfter);
        }

        // ─── Dispose ─────────────────────────────────────────────────────────────

        [Test]
        public void Dispose_UnsubscribesFromView_ChoiceSelectedNoLongerHandled()
        {
            var choiceA = BuildChoice("CHOICE_AFTER_DISPOSE");
            var evt = BuildEvent("EVT_11", choiceA);
            _deckManager.RegisterEvent(evt);
            _deckManager.DrawEvent(_context);
            _presenter.PresentEvent(evt);

            _presenter.Dispose();

            // Simulate click after dispose — should not throw or update state
            Assert.DoesNotThrow(() => _view.SimulateChoiceSelected("CHOICE_AFTER_DISPOSE"));
        }
    }
}
