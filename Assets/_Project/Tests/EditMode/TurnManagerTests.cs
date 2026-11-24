using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features.Finance;
using TheTycoon.Features.Time;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class TurnManagerTests
    {
        [Test]
        public void InitialDateAssignment_MatchesConstructor()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            GameDate start = new(2026, 1);
            TurnManager turnManager = new(ledger, start);

            Assert.AreEqual(start, turnManager.CurrentDate);
            Assert.AreSame(ledger, turnManager.FinancialLedger);
            Assert.IsFalse(turnManager.IsAdvancing);
            Assert.IsFalse(turnManager.IsInitialized);
        }

        [Test]
        public void ServiceLifecycle_InitializeAndShutdown_ManagesState()
        {
            FinancialLedger ledger = new(Money.FromDollars(10000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));

            turnManager.Initialize();
            Assert.IsTrue(turnManager.IsInitialized);

            bool called = false;
            turnManager.OnMonthAdvanced += (s, e) => called = true;

            turnManager.Shutdown();
            Assert.IsFalse(turnManager.IsInitialized);

            // Advancement after shutdown should not call previously cleared subscriber
            turnManager.AdvanceMonth();
            Assert.IsFalse(called);
        }

        [Test]
        public void SuccessfulMonthlyAdvancement_SettlesAccountsAndAdvancesDate()
        {
            FinancialLedger ledger = new(Money.FromDollars(250000m));
            GameDate jan2026 = new(2026, 1);
            TurnManager turnManager = new(ledger, jan2026);

            ledger.AddRevenue("Sales", "Software Licenses", Money.FromDollars(45000m));
            ledger.AddExpense("Operations", "Office & Servers", Money.FromDollars(30000m));

            MonthAdvancedEventArgs receivedArgs = null;
            turnManager.OnMonthAdvanced += (s, e) => receivedArgs = e;

            MonthAdvancedEventArgs returnedArgs = turnManager.AdvanceMonth();

            // Assert calendar progression
            GameDate expectedFeb = new(2026, 2);
            Assert.AreEqual(expectedFeb, turnManager.CurrentDate);

            // Assert financial settlement
            Assert.AreEqual(Money.FromDollars(265000m), ledger.CurrentCash);
            Assert.AreEqual(1, ledger.History.Count);

            // Assert event notification payload
            Assert.IsNotNull(receivedArgs);
            Assert.AreSame(returnedArgs, receivedArgs);
            Assert.AreEqual(expectedFeb, receivedArgs.CurrentDate);
            Assert.AreEqual(jan2026, receivedArgs.SettledSummary.Period);
            Assert.AreEqual(Money.FromDollars(250000m), receivedArgs.SettledSummary.OpeningCash);
            Assert.AreEqual(Money.FromDollars(45000m), receivedArgs.SettledSummary.TotalRevenue);
            Assert.AreEqual(Money.FromDollars(30000m), receivedArgs.SettledSummary.TotalExpenses);
            Assert.AreEqual(Money.FromDollars(15000m), receivedArgs.SettledSummary.NetProfit);
            Assert.AreEqual(Money.FromDollars(265000m), receivedArgs.SettledSummary.ClosingCash);
        }

        [Test]
        public void DecemberToJanuaryProgression_RollsOverYearInEventAndState()
        {
            FinancialLedger ledger = new(Money.FromDollars(100000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 12));

            MonthAdvancedEventArgs args = turnManager.AdvanceMonth();

            Assert.AreEqual(new GameDate(2027, 1), turnManager.CurrentDate);
            Assert.AreEqual(new GameDate(2027, 1), args.CurrentDate);
            Assert.AreEqual(new GameDate(2026, 12), args.SettledSummary.Period);
        }

        [Test]
        public void NotificationOccursAfterStateProgression()
        {
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 3));

            GameDate observedDateDuringEvent = default;
            Money observedCashDuringEvent = default;

            turnManager.OnMonthAdvanced += (s, e) =>
            {
                observedDateDuringEvent = turnManager.CurrentDate;
                observedCashDuringEvent = ledger.CurrentCash;
            };

            turnManager.AdvanceMonth();

            Assert.AreEqual(new GameDate(2026, 4), observedDateDuringEvent);
            Assert.AreEqual(Money.FromDollars(50000m), observedCashDuringEvent);
        }

        [Test]
        public void DuplicateAdvancementProtection_ReentrantCallThrowsInvalidOperationException()
        {
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));

            bool exceptionCaughtInSubscriber = false;

            turnManager.OnMonthAdvanced += (s, e) =>
            {
                // Attempt re-entrant advancement while advancement is in progress
                try
                {
                    turnManager.AdvanceMonth();
                }
                catch (InvalidOperationException)
                {
                    exceptionCaughtInSubscriber = true;
                }
            };

            turnManager.AdvanceMonth();

            Assert.IsTrue(exceptionCaughtInSubscriber, "Expected re-entrant AdvanceMonth call to throw InvalidOperationException.");
            Assert.IsFalse(turnManager.IsAdvancing);
        }

        [Test]
        public void SubscriberUnsubscribeBehavior_DoesNotReceiveSubsequentEvents()
        {
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));

            int callCount = 0;
            EventHandler<MonthAdvancedEventArgs> handler = (s, e) => callCount++;

            turnManager.OnMonthAdvanced += handler;
            turnManager.AdvanceMonth(); // Month 1 -> 2
            Assert.AreEqual(1, callCount);

            turnManager.OnMonthAdvanced -= handler;
            turnManager.AdvanceMonth(); // Month 2 -> 3
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void MultipleSubscribers_AllReceiveNotification()
        {
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));

            bool sub1Called = false;
            bool sub2Called = false;

            turnManager.OnMonthAdvanced += (s, e) => sub1Called = true;
            turnManager.OnMonthAdvanced += (s, e) => sub2Called = true;

            turnManager.AdvanceMonth();

            Assert.IsTrue(sub1Called);
            Assert.IsTrue(sub2Called);
        }

        [Test]
        public void HandlingSubscriberExceptions_OneThrowingDoesNotPreventOthers()
        {
            FinancialLedger ledger = new(Money.FromDollars(50000m));
            TurnManager turnManager = new(ledger, new GameDate(2026, 1));

            bool secondSubscriberInvoked = false;

            turnManager.OnMonthAdvanced += (s, e) =>
            {
                throw new ApplicationException("Simulated subscriber failure.");
            };

            turnManager.OnMonthAdvanced += (s, e) =>
            {
                secondSubscriberInvoked = true;
            };

            Assert.Throws<AggregateException>(() =>
            {
                turnManager.AdvanceMonth();
            });

            Assert.IsTrue(secondSubscriberInvoked, "Second subscriber must be invoked even if first subscriber threw.");
            Assert.IsFalse(turnManager.IsAdvancing);
            Assert.AreEqual(new GameDate(2026, 2), turnManager.CurrentDate);
        }
    }
}
