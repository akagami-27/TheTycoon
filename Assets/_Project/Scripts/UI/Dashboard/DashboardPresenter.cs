using System;
using TheTycoon.Features;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;
using TheTycoon.UI.Events;
using TheTycoon.UI.Finance;

namespace TheTycoon.UI.Dashboard
{
    /// <summary>
    /// Presenter coordinating the company dashboard interface.
    /// Observes TurnManager and SimulationContext, dispatches player commands,
    /// and updates views according to MVP separation of concerns.
    /// Coordinates the Event Modal and Monthly Report Modal to prevent overlap.
    /// </summary>
    public class DashboardPresenter : IDisposable
    {
        private readonly SimulationContext _context;
        private readonly ITopBarView _topBarView;
        private readonly IFinancialOverviewView _financialOverviewView;
        private readonly IMonthlyReportView _monthlyReportView;
        private readonly EventModalPresenter _eventModalPresenter;

        public SimulationContext Context => _context;
        public ITopBarView TopBarView => _topBarView;
        public IFinancialOverviewView FinancialOverviewView => _financialOverviewView;
        public IMonthlyReportView MonthlyReportView => _monthlyReportView;
        public EventModalPresenter EventModalPresenter => _eventModalPresenter;

        public DashboardPresenter(
            SimulationContext context,
            ITopBarView topBarView,
            IFinancialOverviewView financialOverviewView,
            IMonthlyReportView monthlyReportView,
            EventModalPresenter eventModalPresenter = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _topBarView = topBarView ?? throw new ArgumentNullException(nameof(topBarView));
            _financialOverviewView = financialOverviewView ?? throw new ArgumentNullException(nameof(financialOverviewView));
            _monthlyReportView = monthlyReportView ?? throw new ArgumentNullException(nameof(monthlyReportView));
            _eventModalPresenter = eventModalPresenter;
        }

        public void Initialize()
        {
            _topBarView.OnAdvanceClicked += HandleAdvanceClicked;
            _monthlyReportView.OnDismissClicked += HandleDismissReportClicked;
            _context.TurnManager.OnMonthAdvanced += HandleMonthAdvanced;

            if (_eventModalPresenter != null)
            {
                _eventModalPresenter.OnEventResolved += HandleEventResolved;
            }

            RefreshAll();
        }

        public void Dispose()
        {
            if (_topBarView != null)
            {
                _topBarView.OnAdvanceClicked -= HandleAdvanceClicked;
            }

            if (_monthlyReportView != null)
            {
                _monthlyReportView.OnDismissClicked -= HandleDismissReportClicked;
            }

            if (_context?.TurnManager != null)
            {
                _context.TurnManager.OnMonthAdvanced -= HandleMonthAdvanced;
            }

            if (_eventModalPresenter != null)
            {
                _eventModalPresenter.OnEventResolved -= HandleEventResolved;
            }
        }

        public void RefreshAll()
        {
            // 1. Company header & calendar
            _topBarView.SetCompanyName(_context.CompanyProfile.CompanyName);
            _topBarView.SetDate(_context.TurnManager.CurrentDate.ToLongString());
            _topBarView.SetReputation(_context.CorporateReputation);

            // 2. Block advance button when a mandatory event is unresolved
            bool canAdvance = !_context.TurnManager.HasPendingMandatoryEvent;
            _topBarView.SetAdvanceButtonInteractable(canAdvance);

            // 3. Current Cash
            _financialOverviewView.SetCashBalance(_context.CurrentCash.ToFormattedString());

            // 4. Previous month metrics
            MonthlyFinancialSummary latestSummary = _context.FinancialLedger.LatestSummary;
            if (latestSummary == null)
            {
                _financialOverviewView.SetPreviousMonthMetrics(
                    revenueText: "RM0.00",
                    expenseText: "RM0.00",
                    netProfitText: "Not yet available",
                    isAvailable: false,
                    isProfitable: false);
            }
            else
            {
                _financialOverviewView.SetPreviousMonthMetrics(
                    revenueText: latestSummary.TotalRevenue.ToFormattedString(),
                    expenseText: latestSummary.TotalExpenses.ToFormattedString(),
                    netProfitText: latestSummary.NetProfit.ToFormattedString(),
                    isAvailable: true,
                    isProfitable: latestSummary.IsProfitable);
            }
        }

        public void HandleAdvanceClicked()
        {
            if (_context.TurnManager.HasPendingMandatoryEvent)
            {
                // Safety guard: should not reach here since button is disabled, but protect anyway
                _eventModalPresenter?.PresentActiveEvent();
                return;
            }

            _topBarView.SetAdvanceButtonInteractable(false);
            try
            {
                _context.TurnManager.AdvanceMonth();
            }
            finally
            {
                // RefreshAll will update button interactability after event state changes
                RefreshAll();
            }
        }

        private void HandleDismissReportClicked()
        {
            _monthlyReportView.HideReport();

            // If a new event was drawn after month advancement, present it now
            if (_eventModalPresenter != null && _context.TurnManager.HasPendingMandatoryEvent)
            {
                _eventModalPresenter.PresentActiveEvent();
            }
        }

        private void HandleMonthAdvanced(object sender, MonthAdvancedEventArgs e)
        {
            RefreshAll();
            _monthlyReportView.ShowReport(e.SettledSummary);
            // Event modal will be shown after user dismisses the monthly report
        }

        private void HandleEventResolved()
        {
            // Event cleared — refresh dashboard and re-enable advance button
            RefreshAll();
        }
    }
}
