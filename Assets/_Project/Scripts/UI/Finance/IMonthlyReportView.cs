using System;
using TheTycoon.Features.Finance;

namespace TheTycoon.UI.Finance
{
    /// <summary>
    /// View abstraction for the monthly financial accounting report panel.
    /// </summary>
    public interface IMonthlyReportView
    {
        bool IsVisible { get; }
        void ShowReport(MonthlyFinancialSummary summary);
        void HideReport();
        event Action OnDismissClicked;
    }
}
