using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features.Events;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class EventDeckManagerTests
    {
        private class TestFixedRandom : IRandomProvider
        {
            public int NextValue { get; set; }
            public int Next(int minInclusive, int maxExclusive) => NextValue;
        }

        private ChoiceDefinitionSO CreateTestChoice(string id = "C1")
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure(id, "Test Choice");
            return choice;
        }

        [Test]
        public void RegisterEvent_DuplicateId_ThrowsInvalidOperationException()
        {
            var deck = new EventDeckManager();
            var evt1 = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt1.Configure("EVT_01", "Event 1", "Desc", choices: new[] { CreateTestChoice() });

            var evt2 = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt2.Configure("EVT_01", "Event 2", "Desc", choices: new[] { CreateTestChoice() });

            deck.RegisterEvent(evt1);
            Assert.Throws<InvalidOperationException>(() => deck.RegisterEvent(evt2));
        }

        [Test]
        public void GetEligibleEvents_FiltersByCashAndReputationPrerequisites()
        {
            var deck = new EventDeckManager();

            var cashCond = ScriptableObject.CreateInstance<MinimumCashConditionSO>();
            cashCond.SetRequiredCash(Money.FromDollars(50000m));

            var repCond = ScriptableObject.CreateInstance<ReputationConditionSO>();
            repCond.SetReputationRange(60, 100);

            var evtCash = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evtCash.Configure("EVT_CASH", "Expansion", "Desc", prerequisites: new[] { cashCond }, choices: new[] { CreateTestChoice() });

            var evtRep = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evtRep.Configure("EVT_REP", "Award", "Desc", prerequisites: new[] { repCond }, choices: new[] { CreateTestChoice() });

            deck.RegisterEvents(new[] { evtCash, evtRep });

            // Context: Cash = 60,000, Reputation = 40 (meets cash, fails reputation)
            var context = new MockSimulationContext(Money.FromDollars(60000m), new GameDate(2026, 1), 40);

            var eligible = deck.GetEligibleEvents(context);
            Assert.AreEqual(1, eligible.Count);
            Assert.AreEqual("EVT_CASH", eligible[0].EventId);
        }

        [Test]
        public void UniqueEvent_CannotRepeatOnceResolved()
        {
            var deck = new EventDeckManager();
            var uniqueEvt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            uniqueEvt.Configure("EVT_UNIQUE", "IPO Announcement", "Desc", isUnique: true, choices: new[] { CreateTestChoice() });

            deck.RegisterEvent(uniqueEvt);

            var context = new MockSimulationContext(Money.FromDollars(100000m), new GameDate(2026, 1), 50);

            Assert.AreEqual(1, deck.GetEligibleEvents(context).Count);

            // Record as resolved
            deck.RecordEventResolved(uniqueEvt, context.CurrentDate);

            // Should now be excluded
            Assert.AreEqual(0, deck.GetEligibleEvents(context).Count);
        }

        [Test]
        public void CooldownRestriction_ExcludesEventUntilCooldownExpires()
        {
            var deck = new EventDeckManager();
            var coolEvt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            coolEvt.Configure("EVT_AUDIT", "IRS Audit", "Desc", cooldownMonths: 3, choices: new[] { CreateTestChoice() });

            deck.RegisterEvent(coolEvt);

            var context = new MockSimulationContext(Money.FromDollars(100000m), new GameDate(2026, 1), 50);

            // Resolve in Month 1 (Jan 2026). Cooldown is 3 months -> Next available in Month 4 (April 2026).
            deck.RecordEventResolved(coolEvt, context.CurrentDate);

            // Month 2 (Feb): In cooldown
            context.CurrentDate = new GameDate(2026, 2);
            Assert.AreEqual(0, deck.GetEligibleEvents(context).Count);

            // Month 3 (Mar): In cooldown
            context.CurrentDate = new GameDate(2026, 3);
            Assert.AreEqual(0, deck.GetEligibleEvents(context).Count);

            // Month 4 (Apr): Cooldown expired, event is eligible again!
            context.CurrentDate = new GameDate(2026, 4);
            Assert.AreEqual(1, deck.GetEligibleEvents(context).Count);
        }

        [Test]
        public void DrawEvent_EmptyDeck_ReturnsNullGracefully()
        {
            var deck = new EventDeckManager();
            var context = new MockSimulationContext(Money.Zero, new GameDate(2026, 1), 50);

            Assert.IsNull(deck.DrawEvent(context));
            Assert.IsFalse(deck.HasActiveEvent);
        }

        [Test]
        public void DrawEvent_WeightedSelection_SelectsAccordingToInjectedRandomRoll()
        {
            var random = new TestFixedRandom();
            var deck = new EventDeckManager(random);

            var evtA = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evtA.Configure("A", "Common", "Desc", weight: 80, choices: new[] { CreateTestChoice() });

            var evtB = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evtB.Configure("B", "Rare", "Desc", weight: 20, choices: new[] { CreateTestChoice() });

            deck.RegisterEvents(new[] { evtA, evtB });
            var context = new MockSimulationContext(Money.Zero, new GameDate(2026, 1), 50);

            // Total weight = 100. Roll = 10 (< 80) -> Selects A
            random.NextValue = 10;
            var drawn1 = deck.DrawEvent(context);
            Assert.AreEqual("A", drawn1.EventId);

            // Roll = 85 (>= 80) -> Selects B
            random.NextValue = 85;
            var drawn2 = deck.DrawEvent(context);
            Assert.AreEqual("B", drawn2.EventId);
        }
    }
}
