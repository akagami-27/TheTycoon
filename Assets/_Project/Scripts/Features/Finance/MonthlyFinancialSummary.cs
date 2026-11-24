using System;
using TheTycoon.Core;

namespace TheTycoon.Features.Finance
{
    /// <summary>
    /// Immutable representation of a completed monthly financial accounting period.
    /// Preserves revenue, expense, and cash flow results permanently after monthly settlement.
    /// </summary>
    [Serializable]
    public sealed class MonthlyFinancialSummary : IEquatable<MonthlyFinancialSummary>
    {
        public GameDate Period { get; }
        public Money OpeningCash { get; }
        public Money TotalRevenue { get; }
        public Money TotalExpenses { get; }
        public Money NetProfit { get; }
        public Money ClosingCash { get; }

        public bool IsProfitable => NetProfit.IsPositive;
        public bool IsLoss => NetProfit.IsNegative;

        public MonthlyFinancialSummary(
            GameDate period,
            Money openingCash,
            Money totalRevenue,
            Money totalExpenses,
            Money netProfit,
            Money closingCash)
        {
            Period = period;
            OpeningCash = openingCash;
            TotalRevenue = totalRevenue;
            TotalExpenses = totalExpenses;
            NetProfit = netProfit;
            ClosingCash = closingCash;
        }

        public static implicit operator Money(MonthlyFinancialSummary summary)
        {
            return summary?.ClosingCash ?? Money.Zero;
        }

        public bool Equals(MonthlyFinancialSummary other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Period.Equals(other.Period) &&
                   OpeningCash.Equals(other.OpeningCash) &&
                   TotalRevenue.Equals(other.TotalRevenue) &&
                   TotalExpenses.Equals(other.TotalExpenses) &&
                   NetProfit.Equals(other.NetProfit) &&
                   ClosingCash.Equals(other.ClosingCash);
        }

        public override bool Equals(object obj) => obj is MonthlyFinancialSummary other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Period, OpeningCash, TotalRevenue, TotalExpenses, NetProfit, ClosingCash);
        public override string ToString() => $"{Period.ToFormattedString()}: Net {NetProfit.ToFormattedString()}, Closing {ClosingCash.ToFormattedString()}";
    }
}
