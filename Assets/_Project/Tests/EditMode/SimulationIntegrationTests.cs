using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features;
using TheTycoon.Features.Company;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class SimulationIntegrationTests
    {
        [Test]
        public void SimulationContext_IntegratesAllSystemsWithoutStateDuplication()
        {
            GameDate foundingDate = new(2026, 1);
            FinancialLedger ledger = new(Money.FromDollars(100000m));
            TurnManager turnManager = new(ledger, foundingDate);
            CompanyProfile company = new("Cyberdyne Systems", foundingDate, Money.FromDollars(2000000m));

            SimulationContext context = new(ledger, turnManager, company, initialReputation: 60);

            // Assert authoritative references
            Assert.AreSame(ledger, context.FinancialLedger);
            Assert.AreSame(turnManager, context.TurnManager);
            Assert.AreSame(company, context.CompanyProfile);

            // Assert facade values
            Assert.AreEqual(Money.FromDollars(100000m), context.CurrentCash);
            Assert.AreEqual(foundingDate, context.CurrentDate);
            Assert.AreEqual(60, context.CorporateReputation);

            // Test Cash modification strictly modifies FinancialLedger
            context.ModifyCash(Money.FromDollars(25000m));
            Assert.AreEqual(Money.FromDollars(125000m), ledger.CurrentCash);
            Assert.AreEqual(Money.FromDollars(125000m), context.CurrentCash);

            // Test Reputation modification
            context.ModifyReputation(-15);
            Assert.AreEqual(45, context.CorporateReputation);
        }

        [Test]
        public void TurnManager_SettleCompanyMonthlyFinances_DeliversSummaryToSubscribers()
        {
            GameDate startDate = new(2026, 1);
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, startDate);
            CompanyProfile company = new("Global Corp", startDate, Money.FromDollars(500000m));
            SimulationContext context = new(ledger, turnManager, company);

            // Add monthly transactions
            ledger.AddRevenue("Contracts", "Enterprise Contract", Money.FromDollars(40000m));
            ledger.AddExpense("Payroll", "Executive Salaries", Money.FromDollars(25000m));

            MonthlyFinancialSummary receivedSummary = null;
            GameDate newDate = default;

            turnManager.OnMonthAdvanced += (s, e) =>
            {
                receivedSummary = e.SettledSummary;
                newDate = e.CurrentDate;
            };

            // Advance month
            turnManager.AdvanceMonth();

            // Verify date progression in both TurnManager and SimulationContext
            Assert.AreEqual(new GameDate(2026, 2), turnManager.CurrentDate);
            Assert.AreEqual(new GameDate(2026, 2), context.CurrentDate);
            Assert.AreEqual(new GameDate(2026, 2), newDate);

            // Verify cash updated in FinancialLedger and SimulationContext
            Assert.AreEqual(Money.FromDollars(65000m), ledger.CurrentCash);
            Assert.AreEqual(Money.FromDollars(65000m), context.CurrentCash);

            // Verify summary
            Assert.IsNotNull(receivedSummary);
            Assert.AreEqual(startDate, receivedSummary.Period);
            Assert.AreEqual(Money.FromDollars(15000m), receivedSummary.NetProfit);
            Assert.AreEqual(Money.FromDollars(65000m), receivedSummary.ClosingCash);
        }

        [Test]
        public void EventChoiceExecution_DirectlyAltersLedgerThroughContext()
        {
            FinancialLedger ledger = new(Money.FromDollars(100000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));
            CompanyProfile company = new("Apex Holdings", new GameDate(2026, 1), Money.FromDollars(1000000m));
            SimulationContext context = new(ledger, turnManager, company);

            // Choice with cash penalty and condition
            var req = new ICondition[] { new MinimumCashCondition(Money.FromDollars(50000m)) };
            var eff = new IEffect[] { new CashEffect(Money.FromDollars(-30000m)) };
            ChoiceDefinition choice = new("Settle Litigation", req, eff);

            Assert.IsTrue(choice.CanSelect(context));
            choice.Enact(context);

            Assert.AreEqual(Money.FromDollars(70000m), ledger.CurrentCash);
            Assert.AreEqual(Money.FromDollars(70000m), context.CurrentCash);
        }
    }
}
