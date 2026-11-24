using System;
using TheTycoon.Core;
using TheTycoon.Features.Company;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;
using TheTycoon.Features.Workforce;

namespace TheTycoon.Features
{
    /// <summary>
    /// Unified facade providing access to active simulation systems.
    /// Integrates CompanyProfile, FinancialLedger, TurnManager, Event Engine, and WorkforceManager
    /// without duplicating state.
    /// Implements ISimulationContext to drive condition evaluations and event consequences.
    /// </summary>
    public class SimulationContext : ISimulationContext
    {
        private readonly FinancialLedger _financialLedger;
        private readonly TurnManager _turnManager;
        private readonly CompanyProfile _companyProfile;
        private readonly EventDeckManager _eventDeckManager;
        private readonly DelayedEffectScheduler _delayedEffectScheduler;
        private readonly DecisionProcessor _decisionProcessor;
        private readonly WorkforceManager _workforceManager;
        private int _corporateReputation;

        public FinancialLedger FinancialLedger => _financialLedger;
        public TurnManager TurnManager => _turnManager;
        public CompanyProfile CompanyProfile => _companyProfile;
        public EventDeckManager EventDeckManager => _eventDeckManager;
        public DelayedEffectScheduler DelayedEffectScheduler => _delayedEffectScheduler;
        public DecisionProcessor DecisionProcessor => _decisionProcessor;

        /// <summary>
        /// Workforce manager providing authoritative headcount, payroll and productivity data.
        /// May be null when the workforce system has not been initialised.
        /// </summary>
        public WorkforceManager WorkforceManager => _workforceManager;

        public Money CurrentCash => _financialLedger.CurrentCash;
        public GameDate CurrentDate => _turnManager.CurrentDate;
        public int CorporateReputation => _corporateReputation;

        // ─── Convenience workforce accessors ────────────────────────────────────

        /// <summary>Total employee count across all active departments. Zero if no WorkforceManager.</summary>
        public int TotalEmployeeCount => _workforceManager?.TotalEmployeeCount() ?? 0;

        /// <summary>Combined monthly payroll. Zero if no WorkforceManager.</summary>
        public Money TotalMonthlyPayroll => _workforceManager?.TotalMonthlyPayroll() ?? Money.Zero;

        /// <summary>Overall weighted workforce productivity (0–100). Zero if no WorkforceManager.</summary>
        public int WorkforceProductivity => _workforceManager?.OverallProductivity() ?? 0;

        // ─── Constructors ────────────────────────────────────────────────────────

        public SimulationContext(
            FinancialLedger financialLedger,
            TurnManager turnManager,
            CompanyProfile companyProfile,
            int initialReputation = 50)
            : this(financialLedger, turnManager, companyProfile, null, null, null, initialReputation)
        {
        }

        public SimulationContext(
            FinancialLedger financialLedger,
            TurnManager turnManager,
            CompanyProfile companyProfile,
            EventDeckManager eventDeckManager,
            DelayedEffectScheduler delayedEffectScheduler,
            DecisionProcessor decisionProcessor,
            int initialReputation = 50)
            : this(financialLedger, turnManager, companyProfile, eventDeckManager,
                   delayedEffectScheduler, decisionProcessor, null, initialReputation)
        {
        }

        public SimulationContext(
            FinancialLedger financialLedger,
            TurnManager turnManager,
            CompanyProfile companyProfile,
            EventDeckManager eventDeckManager,
            DelayedEffectScheduler delayedEffectScheduler,
            DecisionProcessor decisionProcessor,
            WorkforceManager workforceManager,
            int initialReputation = 50)
        {
            _financialLedger = financialLedger ?? throw new ArgumentNullException(nameof(financialLedger));
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
            _companyProfile = companyProfile ?? throw new ArgumentNullException(nameof(companyProfile));
            _eventDeckManager = eventDeckManager;
            _delayedEffectScheduler = delayedEffectScheduler;
            _decisionProcessor = decisionProcessor;
            _workforceManager = workforceManager;
            _corporateReputation = Math.Clamp(initialReputation, 0, 100);

            // Automatically register context reference on turn manager if not already assigned
            if (_turnManager.SimulationContext == null)
            {
                _turnManager.SimulationContext = this;
            }
        }

        // ─── ISimulationContext ──────────────────────────────────────────────────

        public void ModifyCash(Money delta)
        {
            _financialLedger.ApplyDirectCashDelta(delta);
        }

        public void ModifyReputation(int delta)
        {
            _corporateReputation = Math.Clamp(_corporateReputation + delta, 0, 100);
        }
    }
}
