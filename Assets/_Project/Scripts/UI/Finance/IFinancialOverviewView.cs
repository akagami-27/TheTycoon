namespace TheTycoon.UI.Finance
{
    /// <summary>
    /// View abstraction for displaying company cash balance and previous month's operational performance.
    /// </summary>
    public interface IFinancialOverviewView
    {
        void SetCashBalance(string cashText);
        void SetPreviousMonthMetrics(string revenueText, string expenseText, string netProfitText, bool isAvailable, bool isProfitable);
    }
}
