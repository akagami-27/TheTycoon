using System;
using TheTycoon.Core;
using TheTycoon.Features.Finance;

namespace TheTycoon.Features.Time
{
    /// <summary>
    /// Event arguments published when a monthly turn advances.
    /// Contains the newly active game date and the settled financial summary of the elapsed month.
    /// </summary>
    public sealed class MonthAdvancedEventArgs : EventArgs
    {
        public GameDate CurrentDate { get; }
        public MonthlyFinancialSummary SettledSummary { get; }

        public MonthAdvancedEventArgs(GameDate currentDate, MonthlyFinancialSummary settledSummary)
        {
            CurrentDate = currentDate;
            SettledSummary = settledSummary ?? throw new ArgumentNullException(nameof(settledSummary));
        }
    }
}
