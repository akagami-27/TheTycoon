using System;
using NUnit.Framework;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features;
using TheTycoon.Features.Company;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class EventLifecycleIntegrationTests
    {
        private FinancialLedger _ledger;
        private EventDeckManager _deck;
        private DelayedEffectScheduler _scheduler;
        private DecisionProcessor _processor;
        private TurnManager _turnManager;
        private CompanyProfile _company;
        private SimulationContext _context;

        [SetUp]
        public void Setup()
        {
            GameDate start = new(2026, 1);
            _ledger = new FinancialLedger(Money.FromDollars(200000m));
            _deck = new EventDeckManager();
            _scheduler = new DelayedEffectScheduler();
            _processor = new DecisionProcessor(_deck, _scheduler);
            _turnManager = new TurnManager(_ledger, start, _deck, _scheduler);
            _turnManager.Initialize();
            _company = new CompanyProfile("Apex Global", start, Money.FromDollars(1000000m));
            _context = new SimulationContext(_ledger, _turnManager, _company, _deck, _scheduler, _processor, initialReputation: 50);
        }

        [Test]
        public void MandatoryEvent_BlocksMonthCompletionUntilResolved()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("C1", "Resolve Dispute");

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure("EVT_DISPUTE", "Shareholder Dispute", "Desc", choices: new[] { choice });

            _deck.RegisterEvent(evt);

            // Month begins -> Event drawn
            _turnManager.BeginMonth(_context);

            Assert.IsTrue(_deck.HasActiveEvent);
            Assert.IsTrue(_turnManager.HasPendingMandatoryEvent);

            // Attempting to advance month while event is active must be blocked
            Assert.Throws<InvalidOperationException>(() =>
            {
                _turnManager.AdvanceMonth();
            });

            // Player resolves decision
            DecisionResult result = _processor.ProcessDecision(evt, choice, _context);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(_turnManager.HasPendingMandatoryEvent);

            // Advancing month now succeeds
            MonthAdvancedEventArgs args = _turnManager.AdvanceMonth();
            Assert.AreEqual(new GameDate(2026, 2), _turnManager.CurrentDate);
            Assert.AreEqual(new GameDate(2026, 1), args.SettledSummary.Period);
        }

        [Test]
        public void NoEventAvailable_MonthCanCompleteNormally()
        {
            // Empty deck
            _turnManager.BeginMonth(_context);
            Assert.IsFalse(_turnManager.HasPendingMandatoryEvent);

            // Advance month succeeds without event block
            _turnManager.AdvanceMonth();
            Assert.AreEqual(new GameDate(2026, 2), _turnManager.CurrentDate);
        }

        [Test]
        public void DelayedEffects_ExecuteDuringMonthlyTurnProgressionAtExactDueDate()
        {
            // Schedule delayed effect: +RM50,000 cash in 2 months (March 2026)
            var cashReward = ScriptableObject.CreateInstance<CashEffectSO>();
            cashReward.SetDelta(Money.FromDollars(50000m));

            _scheduler.ScheduleEffect(new GameDate(2026, 1), 2, cashReward, "INVESTMENT_PAYOUT");

            Assert.AreEqual(Money.FromDollars(200000m), _ledger.CurrentCash);

            // Complete Month 1 (Jan -> Feb)
            _turnManager.AdvanceMonth();
            Assert.AreEqual(new GameDate(2026, 2), _turnManager.CurrentDate);
            Assert.AreEqual(Money.FromDollars(200000m), _ledger.CurrentCash); // Not yet due

            // Complete Month 2 (Feb -> Mar)
            _turnManager.AdvanceMonth();
            Assert.AreEqual(new GameDate(2026, 3), _turnManager.CurrentDate);

            // During March advancement (March -> April), the delayed effect is due and executes!
            _turnManager.AdvanceMonth();
            Assert.AreEqual(new GameDate(2026, 4), _turnManager.CurrentDate);
            Assert.AreEqual(Money.FromDollars(250000m), _ledger.CurrentCash); // Executed: +RM50,000
        }
    }
}
