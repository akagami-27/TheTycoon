using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class EventDecisionTests
    {
        /// <summary>
        /// Simulation context harness backed by an authoritative FinancialLedger.
        /// Demonstrates that cash effects modify the single source of truth for company cash.
        /// </summary>
        private class TestSimulationContext : ISimulationContext
        {
            private readonly FinancialLedger _ledger;

            public Money CurrentCash => _ledger.CurrentCash;
            public GameDate CurrentDate { get; }
            public int CorporateReputation { get; private set; }

            public TestSimulationContext(FinancialLedger ledger, int reputation)
            {
                _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
                CurrentDate = new GameDate(2026, 1);
                CorporateReputation = reputation;
            }

            public void ModifyCash(Money delta)
            {
                _ledger.ApplyDirectCashDelta(delta);
            }

            public void ModifyReputation(int delta)
            {
                CorporateReputation += delta;
            }
        }

        [Test]
        public void MinimumCashCondition_EvaluatesCorrectly()
        {
            FinancialLedger richLedger = new(Money.FromDollars(10000m));
            FinancialLedger poorLedger = new(Money.FromDollars(2000m));

            TestSimulationContext contextSufficient = new(richLedger, 50);
            TestSimulationContext contextInsufficient = new(poorLedger, 50);

            MinimumCashCondition condition = new(Money.FromDollars(5000m));

            Assert.IsTrue(condition.Evaluate(contextSufficient));
            Assert.IsFalse(condition.Evaluate(contextInsufficient));
            Assert.IsTrue(condition.GetFailureReason(contextInsufficient).Contains("Requires at least"));
        }

        [Test]
        public void MinimumCashCondition_NullContext_ThrowsArgumentNullException()
        {
            MinimumCashCondition condition = new(Money.FromDollars(5000m));
            Assert.Throws<ArgumentNullException>(() => condition.Evaluate(null));
            Assert.Throws<ArgumentNullException>(() => condition.GetFailureReason(null));
        }

        [Test]
        public void ChoiceDefinition_EnactChoice_AppliesEffectsDirectlyToLedger()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            TestSimulationContext context = new(ledger, 50);

            var requirements = new List<ICondition>
            {
                new MinimumCashCondition(Money.FromDollars(3000m))
            };

            var effects = new List<IEffect>
            {
                new CashEffect(Money.FromDollars(-2000m))
            };

            ChoiceDefinition choice = new("Invest in Marketing", requirements, effects);

            Assert.IsTrue(choice.CanSelect(context));
            choice.Enact(context);

            // Verifies that context and authoritative FinancialLedger are in exact sync
            Assert.AreEqual(Money.FromDollars(8000m), context.CurrentCash);
            Assert.AreEqual(Money.FromDollars(8000m), ledger.CurrentCash);
        }

        [Test]
        public void ChoiceDefinition_EnactWhenRequirementsNotMet_ThrowsInvalidOperationException()
        {
            FinancialLedger ledger = new(Money.FromDollars(1000m));
            TestSimulationContext context = new(ledger, 50);

            var requirements = new List<ICondition>
            {
                new MinimumCashCondition(Money.FromDollars(5000m))
            };

            var effects = new List<IEffect>
            {
                new CashEffect(Money.FromDollars(1000m))
            };

            ChoiceDefinition choice = new("Unmet Option", requirements, effects);

            Assert.IsFalse(choice.CanSelect(context));
            Assert.Throws<InvalidOperationException>(() => choice.Enact(context));
        }

        [Test]
        public void ChoiceDefinition_NullArgumentValidation()
        {
            Assert.Throws<ArgumentException>(() => new ChoiceDefinition(null, null, null));
            Assert.Throws<ArgumentException>(() => new ChoiceDefinition(" ", null, null));

            ChoiceDefinition choice = new("Valid", null, null);
            Assert.Throws<ArgumentNullException>(() => choice.CanSelect(null));
            Assert.Throws<ArgumentNullException>(() => choice.Enact(null));
        }
    }
}
