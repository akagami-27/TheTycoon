using System;
using System.Collections.Generic;
using TheTycoon.Core;

namespace TheTycoon.Features.Finance
{
    /// <summary>
    /// Represents an individual income or expense entry in the monthly ledger.
    /// </summary>
    [Serializable]
    public class LedgerEntry
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public Money Amount { get; set; }

        public LedgerEntry(string category, string description, Money amount)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or whitespace.", nameof(category));
            }

            Category = category;
            Description = description ?? string.Empty;
            Amount = amount;
        }
    }

    /// <summary>
    /// Handles monthly corporate cash flow and serves as the single source of truth for company cash.
    /// Tracks revenues, expenses, net profits, and maintains complete historical accounting summaries.
    /// </summary>
    public class FinancialLedger
    {
        private readonly List<LedgerEntry> _revenueEntries = new();
        private readonly List<LedgerEntry> _expenseEntries = new();
        private readonly List<MonthlyFinancialSummary> _history = new();

        public Money CurrentCash { get; private set; }
        public GameDate? LastSettledPeriod { get; private set; }

        public IReadOnlyList<LedgerEntry> Revenues => _revenueEntries;
        public IReadOnlyList<LedgerEntry> Expenses => _expenseEntries;
        public IReadOnlyList<MonthlyFinancialSummary> History => _history;
        public MonthlyFinancialSummary LatestSummary => _history.Count > 0 ? _history[_history.Count - 1] : null;

        public FinancialLedger(Money startingCash)
        {
            CurrentCash = startingCash;
        }

        public void AddRevenue(string category, string description, Money amount)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Revenue category cannot be null or whitespace.", nameof(category));
            }
            if (amount.IsNegative)
            {
                throw new ArgumentException("Revenue amount cannot be negative.", nameof(amount));
            }
            _revenueEntries.Add(new LedgerEntry(category, description, amount));
        }

        public void AddExpense(string category, string description, Money amount)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Expense category cannot be null or whitespace.", nameof(category));
            }
            if (amount.IsNegative)
            {
                throw new ArgumentException("Expense amount cannot be negative.", nameof(amount));
            }
            _expenseEntries.Add(new LedgerEntry(category, description, amount));
        }

        public Money CalculateTotalRevenue()
        {
            Money total = Money.Zero;
            foreach (var entry in _revenueEntries)
            {
                total += entry.Amount;
            }
            return total;
        }

        public Money CalculateTotalExpenses()
        {
            Money total = Money.Zero;
            foreach (var entry in _expenseEntries)
            {
                total += entry.Amount;
            }
            return total;
        }

        public Money CalculateNetProfit()
        {
            return CalculateTotalRevenue() - CalculateTotalExpenses();
        }

        /// <summary>
        /// Settles accounts for the specified monthly period.
        /// Preserves the completed monthly financial summary, updates cash balance,
        /// and clears active line items for the subsequent month.
        /// Prevents duplicate settlements of the same period.
        /// </summary>
        public MonthlyFinancialSummary SettleMonthlyAccounts(GameDate period)
        {
            if (LastSettledPeriod.HasValue && period <= LastSettledPeriod.Value)
            {
                throw new InvalidOperationException(
                    $"Accounting period {period.ToFormattedString()} has already been settled or is in the past. Last settled period: {LastSettledPeriod.Value.ToFormattedString()}.");
            }

            Money openingCash = CurrentCash;
            Money totalRevenue = CalculateTotalRevenue();
            Money totalExpenses = CalculateTotalExpenses();
            Money netProfit = totalRevenue - totalExpenses;
            Money closingCash = openingCash + netProfit;

            CurrentCash = closingCash;
            LastSettledPeriod = period;

            var summary = new MonthlyFinancialSummary(
                period,
                openingCash,
                totalRevenue,
                totalExpenses,
                netProfit,
                closingCash);

            _history.Add(summary);
            _revenueEntries.Clear();
            _expenseEntries.Clear();

            return summary;
        }

        /// <summary>
        /// Convenience overload settling accounts for the chronological next period (or Year 1 Month 1 if initial).
        /// </summary>
        public MonthlyFinancialSummary SettleMonthlyAccounts()
        {
            GameDate nextPeriod = LastSettledPeriod.HasValue
                ? LastSettledPeriod.Value.AdvanceMonth()
                : new GameDate(2026, 1);

            return SettleMonthlyAccounts(nextPeriod);
        }

        /// <summary>
        /// Direct cash adjustment (e.g. investments, fines, one-off event costs).
        /// Directly updates the single source of truth for company cash.
        /// </summary>
        public void ApplyDirectCashDelta(Money delta)
        {
            checked
            {
                CurrentCash += delta;
            }
        }
    }
}
