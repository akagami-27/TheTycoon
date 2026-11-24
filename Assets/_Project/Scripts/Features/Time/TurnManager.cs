using System;
using System.Collections.Generic;
using TheTycoon.Core;
using TheTycoon.Features.Events;
using TheTycoon.Features.Finance;

namespace TheTycoon.Features.Time
{
    /// <summary>
    /// Coordinates monthly simulation turn progression, calendar advancement,
    /// event lifecycle resolution, delayed consequences, and accounting settlements.
    /// </summary>
    public class TurnManager : IGameService
    {
        private readonly FinancialLedger _financialLedger;
        private readonly EventDeckManager _eventDeckManager;
        private readonly DelayedEffectScheduler _delayedEffectScheduler;
        private GameDate _currentDate;
        private bool _isAdvancing;
        private bool _isInitialized;

        public GameDate CurrentDate => _currentDate;
        public FinancialLedger FinancialLedger => _financialLedger;
        public EventDeckManager EventDeckManager => _eventDeckManager;
        public DelayedEffectScheduler DelayedEffectScheduler => _delayedEffectScheduler;
        public ISimulationContext SimulationContext { get; set; }

        public bool IsAdvancing => _isAdvancing;
        public bool IsInitialized => _isInitialized;

        public bool HasPendingMandatoryEvent => _eventDeckManager != null && _eventDeckManager.HasActiveEvent;

        public event EventHandler<MonthAdvancedEventArgs> OnMonthAdvanced;

        public TurnManager(FinancialLedger financialLedger, GameDate startingDate)
            : this(financialLedger, startingDate, null, null)
        {
        }

        public TurnManager(
            FinancialLedger financialLedger,
            GameDate startingDate,
            EventDeckManager eventDeckManager,
            DelayedEffectScheduler delayedEffectScheduler)
        {
            _financialLedger = financialLedger ?? throw new ArgumentNullException(nameof(financialLedger));
            _currentDate = startingDate;
            _eventDeckManager = eventDeckManager;
            _delayedEffectScheduler = delayedEffectScheduler;
        }

        public void Initialize()
        {
            _isInitialized = true;
        }

        public void Shutdown()
        {
            _isInitialized = false;
            OnMonthAdvanced = null;
        }

        /// <summary>
        /// Begins the monthly event cycle by evaluating eligible events and presenting one if available.
        /// </summary>
        public EventDefinitionSO BeginMonth(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_eventDeckManager == null)
            {
                return null;
            }

            return _eventDeckManager.DrawEvent(context);
        }

        /// <summary>
        /// Advances the simulation to the next monthly turn.
        /// Execution order:
        /// 1. Verify that no mandatory business event remains unresolved.
        /// 2. Apply any due delayed effects for the current month.
        /// 3. Settle the active month's accounting period via FinancialLedger.
        /// 4. Advance the calendar to the subsequent month.
        /// 5. Automatically prepare next month's event opportunity if configured.
        /// 6. Notify all registered subscribers with the new date and previous month's summary.
        /// </summary>
        public MonthAdvancedEventArgs AdvanceMonth()
        {
            if (_isAdvancing)
            {
                throw new InvalidOperationException("Cannot advance month: a turn advancement is already in progress.");
            }

            if (HasPendingMandatoryEvent)
            {
                throw new InvalidOperationException(
                    $"Cannot complete month {_currentDate.ToFormattedString()}: Active event '{_eventDeckManager.ActiveEvent.EventId}' must be resolved first.");
            }

            _isAdvancing = true;
            try
            {
                // 1. Process due delayed effects for the current month
                if (_delayedEffectScheduler != null && SimulationContext != null)
                {
                    _delayedEffectScheduler.ProcessDueEffects(_currentDate, SimulationContext);
                }

                // 2. Settle current monthly accounting period
                MonthlyFinancialSummary summary = _financialLedger.SettleMonthlyAccounts(_currentDate);

                // 3. Advance calendar to next month
                _currentDate = _currentDate.AdvanceMonth();

                // 4. Optionally draw next month's event if deck manager and context are active
                if (_eventDeckManager != null && SimulationContext != null && !_eventDeckManager.HasActiveEvent)
                {
                    _eventDeckManager.DrawEvent(SimulationContext);
                }

                // 5. Prepare event payload
                var args = new MonthAdvancedEventArgs(_currentDate, summary);

                // 6. Publish notification safely across all subscribers
                PublishMonthAdvanced(args);

                return args;
            }
            finally
            {
                _isAdvancing = false;
            }
        }

        private void PublishMonthAdvanced(MonthAdvancedEventArgs args)
        {
            var handler = OnMonthAdvanced;
            if (handler == null)
            {
                return;
            }

            var invocationList = handler.GetInvocationList();
            List<Exception> subscriberExceptions = null;

            foreach (var subscriber in invocationList)
            {
                try
                {
                    ((EventHandler<MonthAdvancedEventArgs>)subscriber)(this, args);
                }
                catch (Exception ex)
                {
                    subscriberExceptions ??= new List<Exception>();
                    subscriberExceptions.Add(ex);
                }
            }

            if (subscriberExceptions != null && subscriberExceptions.Count > 0)
            {
                throw new AggregateException(
                    "One or more exceptions occurred while notifying subscribers of month advancement.",
                    subscriberExceptions);
            }
        }
    }
}
