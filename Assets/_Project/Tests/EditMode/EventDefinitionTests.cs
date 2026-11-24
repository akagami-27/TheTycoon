using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features.Events;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class EventDefinitionTests
    {
        [Test]
        public void ValidEventDefinition_InitializesCorrectly()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("CHOICE_A", "Accept Offer", "Accept the business proposal");

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            evt.Configure(
                "EVT_001",
                "Seed Capital Proposal",
                "An angel investor offers initial backing.",
                EventCategory.Financial,
                weight: 150,
                isUnique: true,
                cooldownMonths: 12,
                choices: new[] { choice });

            Assert.AreEqual("EVT_001", evt.EventId);
            Assert.AreEqual("Seed Capital Proposal", evt.Title);
            Assert.AreEqual(EventCategory.Financial, evt.Category);
            Assert.AreEqual(150, evt.Weight);
            Assert.IsTrue(evt.IsUnique);
            Assert.AreEqual(12, evt.CooldownMonths);
            Assert.AreEqual(1, evt.Choices.Count);
            Assert.IsTrue(evt.ContainsChoice(choice));
        }

        [Test]
        public void InvalidEventIdentifier_ThrowsArgumentException()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("CHOICE_A", "Valid");

            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();

            Assert.Throws<ArgumentException>(() => evt.Configure("", "Title", "Desc", choices: new[] { choice }));
            Assert.Throws<ArgumentException>(() => evt.Configure("   ", "Title", "Desc", choices: new[] { choice }));
            Assert.Throws<ArgumentException>(() => evt.Configure(null, "Title", "Desc", choices: new[] { choice }));
        }

        [Test]
        public void EventWithoutChoices_ThrowsArgumentException()
        {
            var evt = ScriptableObject.CreateInstance<EventDefinitionSO>();
            Assert.Throws<ArgumentException>(() => evt.Configure("EVT_EMPTY", "Title", "Desc", choices: Array.Empty<ChoiceDefinitionSO>()));
        }

        [Test]
        public void ChoiceDefinition_ValidAndInvalidConfiguration()
        {
            var choice = ScriptableObject.CreateInstance<ChoiceDefinitionSO>();
            choice.Configure("CH_01", "Sign Agreement");

            Assert.AreEqual("CH_01", choice.ChoiceId);
            Assert.AreEqual("Sign Agreement", choice.ChoiceTitle);

            Assert.Throws<ArgumentException>(() => choice.Configure("", "Title"));
            Assert.Throws<ArgumentException>(() => choice.Configure("CH_02", ""));
        }

        [Test]
        public void ReputationCondition_EvaluatesRangeCorrectly()
        {
            var condition = ScriptableObject.CreateInstance<ReputationConditionSO>();
            condition.SetReputationRange(40, 80);

            // Mock simulation context
            var lowContext = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 30);
            var validContext = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 60);
            var highContext = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 90);

            Assert.IsFalse(condition.Evaluate(lowContext));
            Assert.IsTrue(condition.Evaluate(validContext));
            Assert.IsFalse(condition.Evaluate(highContext));
        }

        [Test]
        public void DateCondition_EvaluatesCalendarBounds()
        {
            var condition = ScriptableObject.CreateInstance<DateConditionSO>();
            condition.SetMinimumDate(new GameDate(2026, 6));
            condition.SetMaximumDate(new GameDate(2027, 6));

            var early = new MockSimulationContext(Money.Zero, new GameDate(2026, 3), 50);
            var mid = new MockSimulationContext(Money.Zero, new GameDate(2026, 10), 50);
            var late = new MockSimulationContext(Money.Zero, new GameDate(2027, 8), 50);

            Assert.IsFalse(condition.Evaluate(early));
            Assert.IsTrue(condition.Evaluate(mid));
            Assert.IsFalse(condition.Evaluate(late));
        }
    }

    internal class MockSimulationContext : ISimulationContext
    {
        public Money CurrentCash { get; private set; }
        public GameDate CurrentDate { get; set; }
        public int CorporateReputation { get; private set; }

        public MockSimulationContext(Money cash, GameDate date, int reputation)
        {
            CurrentCash = cash;
            CurrentDate = date;
            CorporateReputation = reputation;
        }

        public void ModifyCash(Money delta) => CurrentCash += delta;
        public void ModifyReputation(int delta) => CorporateReputation = Math.Clamp(CorporateReputation + delta, 0, 100);
    }
}
