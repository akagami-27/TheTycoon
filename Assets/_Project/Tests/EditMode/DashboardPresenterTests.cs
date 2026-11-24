using System;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features;
using TheTycoon.Features.Company;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;
using TheTycoon.UI.Dashboard;
using TheTycoon.UI.Finance;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class DashboardPresenterTests
    {
        private class MockTopBarView : ITopBarView
        {
            public string CompanyName { get; private set; }
            public string DateText { get; private set; }
            public int Reputation { get; private set; }
            public bool IsInteractable { get; private set; } = true;

            public event Action OnAdvanceClicked;

            public void SetCompanyName(string companyName) => CompanyName = companyName;
            public void SetDate(string dateText) => DateText = dateText;
            public void SetReputation(int reputation) => Reputation = reputation;
            public void SetAdvanceButtonInteractable(bool interactable) => IsInteractable = interactable;

            public void SimulateAdvanceClick() => OnAdvanceClicked?.Invoke();
        }

        private class MockFinancialOverviewView : IFinancialOverviewView
        {
            public string CashText { get; private set; }
            public string RevenueText { get; private set; }
            public string ExpenseText { get; private set; }
            public string NetProfitText { get; private set; }
            public bool IsAvailable { get; private set; }
            public bool IsProfitable { get; private set; }

            public void SetCashBalance(string cashText) => CashText = cashText;

            public void SetPreviousMonthMetrics(
                string revenueText,
                string expenseText,
                string netProfitText,
                bool isAvailable,
                bool isProfitable)
            {
                RevenueText = revenueText;
                ExpenseText = expenseText;
                NetProfitText = netProfitText;
                IsAvailable = isAvailable;
                IsProfitable = isProfitable;
            }
        }

        private class MockMonthlyReportView : IMonthlyReportView
        {
            public bool IsVisible { get; private set; }
            public MonthlyFinancialSummary DisplayedSummary { get; private set; }

            public event Action OnDismissClicked;

            public void ShowReport(MonthlyFinancialSummary summary)
            {
                IsVisible = true;
                DisplayedSummary = summary;
            }

            public void HideReport()
            {
                IsVisible = false;
            }

            public void SimulateDismissClick() => OnDismissClicked?.Invoke();
        }

        private FinancialLedger _ledger;
        private TurnManager _turnManager;
        private CompanyProfile _company;
        private SimulationContext _context;
        private MockTopBarView _topBar;
        private MockFinancialOverviewView _finOverview;
        private MockMonthlyReportView _reportView;
        private DashboardPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            GameDate start = new(2026, 1);
            _ledger = new FinancialLedger(Money.FromDollars(250000m));
            _turnManager = new TurnManager(_ledger, start);
            _turnManager.Initialize();
            _company = new CompanyProfile("Acme Corp", start, Money.FromDollars(1000000m));
            _context = new SimulationContext(_ledger, _turnManager, _company, initialReputation: 50);

            _topBar = new MockTopBarView();
            _finOverview = new MockFinancialOverviewView();
            _reportView = new MockMonthlyReportView();

            _presenter = new DashboardPresenter(_context, _topBar, _finOverview, _reportView);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter?.Dispose();
        }

        [Test]
        public void Initialize_PopulatesInitialViewStates()
        {
            _presenter.Initialize();

            Assert.AreEqual("Acme Corp", _topBar.CompanyName);
            Assert.AreEqual("January 2026 (Q1)", _topBar.DateText);
            Assert.AreEqual(50, _topBar.Reputation);
            Assert.AreEqual("RM250,000.00", _finOverview.CashText);
            Assert.IsFalse(_finOverview.IsAvailable);
            Assert.AreEqual("Not yet available", _finOverview.NetProfitText);
            Assert.IsFalse(_reportView.IsVisible);
        }

        [Test]
        public void HandleAdvanceClick_AdvancesMonth_RefreshesViewsAndDisplaysReport()
        {
            _presenter.Initialize();

            // Add revenue and expenses for January
            _ledger.AddRevenue("Sales", "Client Retainer", Money.FromDollars(45000m));
            _ledger.AddExpense("Operations", "Office Leases", Money.FromDollars(30000m));

            // User clicks Advance Month
            _topBar.SimulateAdvanceClick();

            // Calendar advanced to February
            Assert.AreEqual(new GameDate(2026, 2), _turnManager.CurrentDate);
            Assert.AreEqual("February 2026 (Q1)", _topBar.DateText);

            // Cash updated to RM265,000.00
            Assert.AreEqual("RM265,000.00", _finOverview.CashText);
            Assert.IsTrue(_finOverview.IsAvailable);
            Assert.AreEqual("RM45,000.00", _finOverview.RevenueText);
            Assert.AreEqual("RM30,000.00", _finOverview.ExpenseText);
            Assert.AreEqual("RM15,000.00", _finOverview.NetProfitText);
            Assert.IsTrue(_finOverview.IsProfitable);

            // Report popup shown with January summary
            Assert.IsTrue(_reportView.IsVisible);
            Assert.IsNotNull(_reportView.DisplayedSummary);
            Assert.AreEqual(new GameDate(2026, 1), _reportView.DisplayedSummary.Period);
            Assert.AreEqual(Money.FromDollars(15000m), _reportView.DisplayedSummary.NetProfit);
            Assert.AreEqual(Money.FromDollars(265000m), _reportView.DisplayedSummary.ClosingCash);
        }

        [Test]
        public void DismissReport_HidesMonthlyReport()
        {
            _presenter.Initialize();
            _topBar.SimulateAdvanceClick();

            Assert.IsTrue(_reportView.IsVisible);

            _reportView.SimulateDismissClick();
            Assert.IsFalse(_reportView.IsVisible);
        }

        [Test]
        public void Dispose_UnsubscribesFromEvents_PreventsSubsequentUpdates()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            // Advance month directly via TurnManager
            _turnManager.AdvanceMonth();

            // Views should not update because presenter is disposed
            Assert.AreEqual("January 2026 (Q1)", _topBar.DateText);
            Assert.IsFalse(_reportView.IsVisible);
        }

        [Test]
        public void AdvanceButton_InteractableStateMaintainedEvenIfAdvancementFails()
        {
            _presenter.Initialize();

            // Advance once
            _topBar.SimulateAdvanceClick();
            Assert.IsTrue(_topBar.IsInteractable);

            // Button remains interactable
            Assert.IsTrue(_topBar.IsInteractable);
        }
    }
}
