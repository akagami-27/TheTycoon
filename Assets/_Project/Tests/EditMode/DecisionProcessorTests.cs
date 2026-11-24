using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class DecisionProcessorTests
    {
        private EventDeckManager _deck;
        private DelayedEffectScheduler _scheduler;
        private DecisionProcessor _processor;
        private FinancialLedger _ledger;
        private MockSimulationContext _context;

        [SetUp]
        public void Setup()
        {
            _deck = new EventDeckManager();
            _scheduler = new DelayedEffectScheduler();
            _processor = new DecisionProcessor(_deck, _scheduler);
            _ledger = new FinancialLedger(Money.FromDollars(100000m));
            _context = new MockSimulationContext(Money.FromDollars(100000m), new GameDate(2026, 1), 50);
        }

        [Test]
        public void ProcessDecision_ValidChoice_EnactsImmediateAndSchedulesDelayed()
        {
            var cashEffect = ScriptableObject.CreateInstance<CashEffectSO>();
            cashEffect.SetDelta(Money.FromDollars(-20000m));

            var repEffect = ScriptableObject.CreateInstance<ReputationEffectSO>();
            repEffect.SetDeltaReputation(15);
            var delayedDef = new DelayedEffectDefinition(repEffect, delayMonths: 2);

            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("CH_TRAIN", "Staff Training", immediateEffects: new[] { cashEffect }, delayedEffects: new[] { delayedDef });

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure("EVT_TRAIN", "Training Initiative", "Desc", choices: new[] { choice });

            _deck.RegisterEvent(evt);
            _deck.DrawEvent(_context); // Sets as active event

            DecisionResult result = _processor.ProcessDecision(evt, choice, _context);

            // Assert success
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.ErrorMessage);

            // Immediate effect applied
            Assert.AreEqual(Money.FromDollars(80000m), _context.CurrentCash);

            // Delayed effect queued in scheduler
            Assert.AreEqual(1, _scheduler.PendingCount);
            Assert.AreEqual(new GameDate(2026, 3), _scheduler.PendingEffects[0].DueDate);

            // Event resolved in deck manager
            Assert.IsFalse(_deck.HasActiveEvent);
            Assert.IsTrue(_deck.IsEventResolved("EVT_TRAIN"));
        }

        [Test]
        public void ProcessDecision_ChoiceFromAnotherEvent_Rejected()
        {
            var choice1 = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice1.Configure("C1", "Option 1");

            var choiceForeign = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choiceForeign.Configure("CFOREIGN", "Foreign Option");

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure("EVT_MAIN", "Main Event", "Desc", choices: new[] { choice1 });

            _deck.RegisterEvent(evt);
            _deck.DrawEvent(_context);

            DecisionResult result = _processor.ProcessDecision(evt, choiceForeign, _context);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("does not belong to event"));
            Assert.AreEqual(Money.FromDollars(100000m), _context.CurrentCash);
        }

        [Test]
        public void ProcessDecision_UnmetPrerequisites_RejectedWithoutModifyingState()
        {
            var cashReq = ScriptableObject.CreateInstance<MinimumCashConditionSO>();
            cashReq.SetRequiredCash(Money.FromDollars(200000m)); // Context only has 100,000

            var cashEffect = ScriptableObject.CreateInstance<CashEffectSO>();
            cashEffect.SetDelta(Money.FromDollars(-10000m));

            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("CH_EXPENSIVE", "Mega Deal", requirements: new[] { cashReq }, immediateEffects: new[] { cashEffect });

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure("EVT_DEAL", "Deal Proposal", "Desc", choices: new[] { choice });

            _deck.RegisterEvent(evt);
            _deck.DrawEvent(_context);

            DecisionResult result = _processor.ProcessDecision(evt, choice, _context);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("Requirements for choice"));
            Assert.AreEqual(Money.FromDollars(100000m), _context.CurrentCash);
            Assert.IsTrue(_deck.HasActiveEvent);
        }

        [Test]
        public void ProcessDecision_EventNotActive_Rejected()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("C1", "Choice");

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure("EVT_INACTIVE", "Inactive", "Desc", choices: new[] { choice });

            // Do not draw event -> active event is null
            DecisionResult result = _processor.ProcessDecision(evt, choice, _context);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.ErrorMessage.Contains("not the currently active event"));
        }
    }
}
