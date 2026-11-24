using System;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features.Finance;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class FinancialLedgerTests
    {
        [Test]
        public void StartingCash_InitializesAccurately()
        {
            Money initial = Money.FromDollars(50000m);
            FinancialLedger ledger = new(initial);

            Assert.AreEqual(initial, ledger.CurrentCash);
            Assert.AreEqual(Money.Zero, ledger.CalculateTotalRevenue());
            Assert.AreEqual(Money.Zero, ledger.CalculateTotalExpenses());
            Assert.AreEqual(0, ledger.History.Count);
            Assert.IsNull(ledger.LatestSummary);
            Assert.IsNull(ledger.LastSettledPeriod);
        }

        [Test]
        public void AddRevenue_ValidAndInvalidEntries()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));

            ledger.AddRevenue("Sales", "Subscription Income", Money.FromDollars(5000m));
            Assert.AreEqual(1, ledger.Revenues.Count);
            Assert.AreEqual(Money.FromDollars(5000m), ledger.CalculateTotalRevenue());

            Assert.Throws<ArgumentException>(() => ledger.AddRevenue("", "Invalid", Money.FromDollars(100m)));
            Assert.Throws<ArgumentException>(() => ledger.AddRevenue("Sales", "Negative", Money.FromDollars(-100m)));
        }

        [Test]
        public void AddExpense_ValidAndInvalidEntries()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));

            ledger.AddExpense("Payroll", "Staff Salary", Money.FromDollars(3000m));
            Assert.AreEqual(1, ledger.Expenses.Count);
            Assert.AreEqual(Money.FromDollars(3000m), ledger.CalculateTotalExpenses());

            Assert.Throws<ArgumentException>(() => ledger.AddExpense(null, "Invalid", Money.FromDollars(100m)));
            Assert.Throws<ArgumentException>(() => ledger.AddExpense("Operations", "Negative", Money.FromDollars(-100m)));
        }

        [Test]
        public void AddRevenueAndExpenses_CalculatesTotalsAndNetProfit()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));

            ledger.AddRevenue("Product", "Standard Unit Sales", Money.FromDollars(8000m));
            ledger.AddRevenue("Services", "Consulting Fee", Money.FromDollars(2000m));

            ledger.AddExpense("Operations", "Office Rent", Money.FromDollars(3000m));
            ledger.AddExpense("Payroll", "Base Salaries", Money.FromDollars(4000m));

            Assert.AreEqual(Money.FromDollars(10000m), ledger.CalculateTotalRevenue());
            Assert.AreEqual(Money.FromDollars(7000m), ledger.CalculateTotalExpenses());
            Assert.AreEqual(Money.FromDollars(3000m), ledger.CalculateNetProfit());
        }

        [Test]
        public void CalculateNetLoss_WhenExpensesExceedRevenue()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));

            ledger.AddRevenue("Sales", "Low Sales", Money.FromDollars(1000m));
            ledger.AddExpense("Rent", "Office Lease", Money.FromDollars(4000m));

            Assert.AreEqual(Money.FromDollars(-3000m), ledger.CalculateNetProfit());
        }

        [Test]
        public void SettleMonthlyAccounts_UpdatesCashAndPreservesSummary()
        {
            Money initialCash = Money.FromDollars(10000m);
            FinancialLedger ledger = new(initialCash);
            GameDate period = new(2026, 1);

            ledger.AddRevenue("Sales", "Product Sales", Money.FromDollars(5000m));
            ledger.AddExpense("Costs", "Operations", Money.FromDollars(2000m));

            MonthlyFinancialSummary summary = ledger.SettleMonthlyAccounts(period);

            Assert.IsNotNull(summary);
            Assert.AreEqual(period, summary.Period);
            Assert.AreEqual(initialCash, summary.OpeningCash);
            Assert.AreEqual(Money.FromDollars(5000m), summary.TotalRevenue);
            Assert.AreEqual(Money.FromDollars(2000m), summary.TotalExpenses);
            Assert.AreEqual(Money.FromDollars(3000m), summary.NetProfit);
            Assert.AreEqual(Money.FromDollars(13000m), summary.ClosingCash);
            Assert.IsTrue(summary.IsProfitable);
            Assert.IsFalse(summary.IsLoss);

            // Verify ledger state
            Assert.AreEqual(Money.FromDollars(13000m), ledger.CurrentCash);
            Assert.AreEqual(period, ledger.LastSettledPeriod);
            Assert.AreEqual(1, ledger.History.Count);
            Assert.AreSame(summary, ledger.LatestSummary);

            // Active monthly transaction lines must be cleared
            Assert.AreEqual(0, ledger.Revenues.Count);
            Assert.AreEqual(0, ledger.Expenses.Count);
        }

        [Test]
        public void SettleMonthlyAccounts_DuplicatePeriodSettlement_ThrowsInvalidOperationException()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            GameDate jan2026 = new(2026, 1);

            ledger.SettleMonthlyAccounts(jan2026);

            Assert.Throws<InvalidOperationException>(() =>
            {
                ledger.SettleMonthlyAccounts(jan2026);
            });
        }

        [Test]
        public void SettleMonthlyAccounts_PastPeriodSettlement_ThrowsInvalidOperationException()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            ledger.SettleMonthlyAccounts(new GameDate(2026, 5));

            Assert.Throws<InvalidOperationException>(() =>
            {
                ledger.SettleMonthlyAccounts(new GameDate(2026, 4));
            });
        }

        [Test]
        public void ConsecutiveMonthlySettlements_PreservesHistoricalSequence()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));

            // Month 1
            ledger.AddRevenue("Sales", "Month 1 Sales", Money.FromDollars(5000m));
            MonthlyFinancialSummary m1 = ledger.SettleMonthlyAccounts(new GameDate(2026, 1));

            // Month 2
            ledger.AddExpense("Legal", "Consulting", Money.FromDollars(3000m));
            MonthlyFinancialSummary m2 = ledger.SettleMonthlyAccounts(new GameDate(2026, 2));

            Assert.AreEqual(2, ledger.History.Count);
            Assert.AreEqual(Money.FromDollars(10000m), m1.OpeningCash);
            Assert.AreEqual(Money.FromDollars(15000m), m1.ClosingCash);

            Assert.AreEqual(Money.FromDollars(15000m), m2.OpeningCash);
            Assert.AreEqual(Money.FromDollars(12000m), m2.ClosingCash);
            Assert.AreEqual(Money.FromDollars(12000m), ledger.CurrentCash);
            Assert.AreSame(m2, ledger.LatestSummary);
        }

        [Test]
        public void ApplyDirectCashDelta_ModifiesAuthoritativeBalanceDirectly()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            ledger.ApplyDirectCashDelta(Money.FromDollars(-2500m));

            Assert.AreEqual(Money.FromDollars(7500m), ledger.CurrentCash);
        }
    }
}
