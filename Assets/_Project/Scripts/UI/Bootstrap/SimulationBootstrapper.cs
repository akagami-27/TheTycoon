using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features;
using TheTycoon.Features.Company;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;
using TheTycoon.Features.Workforce;
using TheTycoon.UI.Dashboard;
using TheTycoon.UI.Events;
using TheTycoon.UI.Finance;

namespace TheTycoon.UI.Bootstrap
{
    /// <summary>
    /// Explicit initialization component for the Game scene.
    /// Bootstraps simulation dependencies in a deterministic order and wires up
    /// the DashboardPresenter together with the EventModalPresenter.
    ///
    /// Payroll lifecycle:
    ///   WorkforceManager.RecordPayrollExpense() is called once at the start of each
    ///   month period — immediately before AdvanceMonth() settles the ledger.
    ///   This is done by subscribing to TurnManager.OnMonthAdvanced and recording payroll
    ///   for the NEXT month immediately after the previous month completes.
    ///   For month 1 (startup), payroll is recorded in InitializeSimulation().
    /// </summary>
    public class SimulationBootstrapper : MonoBehaviour
    {
        [Header("Dashboard View References")]
        [SerializeField] private TopBarView _topBarView;
        [SerializeField] private FinancialOverviewView _financialOverviewView;
        [SerializeField] private MonthlyReportView _monthlyReportView;

        [Header("Event System References")]
        [SerializeField] private EventModalView _eventModalView;
        [SerializeField] private EventDefinitionSO[] _sampleEvents;

        private DashboardPresenter _presenter;
        private SimulationContext _simulationContext;
        private EventModalPresenter _eventModalPresenter;

        public DashboardPresenter Presenter => _presenter;
        public SimulationContext SimulationContext => _simulationContext;
        public EventModalPresenter EventModalPresenter => _eventModalPresenter;

        private void Awake()
        {
            InitializeSimulation();
        }

        private void OnDestroy()
        {
            if (_simulationContext?.TurnManager != null)
            {
                _simulationContext.TurnManager.OnMonthAdvanced -= HandleMonthAdvancedForPayroll;
            }

            _eventModalPresenter?.Dispose();
            _presenter?.Dispose();
        }

        public void InitializeSimulation(SimulationContext existingContext = null)
        {
            if (existingContext != null)
            {
                _simulationContext = existingContext;
            }
            else
            {
                GameDate startDate = new(2026, 1);
                Money startingCash = Money.FromDollars(250000m);     // RM250,000.00
                Money initialValuation = Money.FromDollars(1000000m); // RM1,000,000.00

                var ledger = new FinancialLedger(startingCash);

                // Initial recurring revenue for month 1
                ledger.AddRevenue("Retainers", "Client Consulting Retainers", Money.FromDollars(45000m));

                // Build workforce — this replaces the old hardcoded payroll line
                var workforce = BuildInitialWorkforce();

                // Record month 1 payroll via WorkforceManager before first advance
                workforce.RecordPayrollExpense(ledger, startDate);

                // Build event engine subsystems
                var deckManager = new EventDeckManager();
                var scheduler = new DelayedEffectScheduler();
                var decisionProcessor = new DecisionProcessor(deckManager, scheduler);
                var turnManager = new TurnManager(ledger, startDate, deckManager, scheduler);
                turnManager.Initialize();

                var company = new CompanyProfile("Nexus Corporation", startDate, initialValuation);

                _simulationContext = new SimulationContext(
                    ledger,
                    turnManager,
                    company,
                    deckManager,
                    scheduler,
                    decisionProcessor,
                    workforce,
                    initialReputation: 50);
            }

            // Subscribe: record payroll for each new month after month-advance
            _simulationContext.TurnManager.OnMonthAdvanced += HandleMonthAdvancedForPayroll;

            // Register sample event assets into the deck
            if (_sampleEvents != null && _simulationContext.EventDeckManager != null)
            {
                foreach (var evt in _sampleEvents)
                {
                    if (evt != null)
                    {
                        _simulationContext.EventDeckManager.RegisterEvent(evt);
                    }
                }
            }

            // Wire up Event Modal Presenter
            if (_eventModalView != null && _simulationContext.DecisionProcessor != null)
            {
                _eventModalPresenter = new EventModalPresenter(
                    _simulationContext,
                    _simulationContext.DecisionProcessor,
                    _eventModalView);
            }

            // Wire up Dashboard Presenter
            if (_topBarView != null && _financialOverviewView != null && _monthlyReportView != null)
            {
                _presenter = new DashboardPresenter(
                    _simulationContext,
                    _topBarView,
                    _financialOverviewView,
                    _monthlyReportView,
                    _eventModalPresenter);

                _presenter.Initialize();
            }

            // Begin the first month — draw an event if one is eligible
            if (_simulationContext.EventDeckManager != null)
            {
                var drawnEvent = _simulationContext.TurnManager.BeginMonth(_simulationContext);
                if (drawnEvent != null && _eventModalPresenter != null)
                {
                    _eventModalPresenter.PresentActiveEvent();
                }
            }
        }

        /// <summary>
        /// Subscribes to OnMonthAdvanced so payroll is recorded for the new month
        /// immediately after the previous month's settlement completes.
        /// The new month date is available on the TurnManager after advancement.
        /// </summary>
        private void HandleMonthAdvancedForPayroll(object sender, MonthAdvancedEventArgs e)
        {
            var workforce = _simulationContext?.WorkforceManager;
            var ledger = _simulationContext?.FinancialLedger;
            if (workforce == null || ledger == null)
            {
                return;
            }

            GameDate newPeriod = _simulationContext.CurrentDate;

            // Guard: only record if not already set for this period
            if (!workforce.IsPayrollRecordedFor(newPeriod))
            {
                workforce.RecordPayrollExpense(ledger, newPeriod);
            }
        }

        /// <summary>
        /// Creates the initial prototype workforce for Nexus Corporation.
        /// Values reflect a small consulting firm at game start.
        /// </summary>
        private static WorkforceManager BuildInitialWorkforce()
        {
            var workforce = new WorkforceManager();

            workforce.AddDepartment(new Department(
                DepartmentType.Management, "Management",
                initialEmployeeCount: 3,
                averageMonthlySalary: Money.FromDollars(8000m),
                initialProductivity: 80));

            workforce.AddDepartment(new Department(
                DepartmentType.Finance, "Finance",
                initialEmployeeCount: 4,
                averageMonthlySalary: Money.FromDollars(4500m),
                initialProductivity: 75));

            workforce.AddDepartment(new Department(
                DepartmentType.HumanResources, "Human Resources",
                initialEmployeeCount: 3,
                averageMonthlySalary: Money.FromDollars(4000m),
                initialProductivity: 70));

            workforce.AddDepartment(new Department(
                DepartmentType.Operations, "Operations",
                initialEmployeeCount: 12,
                averageMonthlySalary: Money.FromDollars(3500m),
                initialProductivity: 72));

            workforce.AddDepartment(new Department(
                DepartmentType.SalesAndMarketing, "Sales & Marketing",
                initialEmployeeCount: 6,
                averageMonthlySalary: Money.FromDollars(4000m),
                initialProductivity: 68));

            workforce.AddDepartment(new Department(
                DepartmentType.ResearchAndDevelopment, "Research & Development",
                initialEmployeeCount: 5,
                averageMonthlySalary: Money.FromDollars(5000m),
                initialProductivity: 78));

            return workforce;
        }
    }
}
