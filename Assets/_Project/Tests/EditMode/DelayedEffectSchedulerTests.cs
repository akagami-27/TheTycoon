using System;
using NUnit.Framework;
using UnityEngine;
using TheTycoon.Core;
using TheTycoon.Features.Events;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class DelayedEffectSchedulerTests
    {
        [Test]
        public void ScheduleEffect_CalculatesExactDueDate()
        {
            var scheduler = new DelayedEffectScheduler();
            var effect = ScriptableObject.CreateInstance<ReputationEffectSO>();
            effect.SetDeltaReputation(10);

            GameDate currentDate = new(2026, 1);
            var entry = scheduler.ScheduleEffect(currentDate, 3, effect, "EVT_001");

            Assert.AreEqual(1, scheduler.PendingCount);
            Assert.AreEqual(new GameDate(2026, 4), entry.DueDate);
            Assert.AreEqual("EVT_001", entry.SourceEventId);
        }

        [Test]
        public void ProcessDueEffects_DoesNotExecutePrematurely()
        {
            var scheduler = new DelayedEffectScheduler();
            var context = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 50);

            var effect = ScriptableObject.CreateInstance<ReputationEffectSO>();
            effect.SetDeltaReputation(20);

            scheduler.ScheduleEffect(context.CurrentDate, 3, effect); // Due in April (Month 4)

            // Month 1 (Jan)
            int executed1 = scheduler.ProcessDueEffects(new GameDate(2026, 1), context);
            Assert.AreEqual(0, executed1);
            Assert.AreEqual(50, context.CorporateReputation);

            // Month 2 (Feb)
            int executed2 = scheduler.ProcessDueEffects(new GameDate(2026, 2), context);
            Assert.AreEqual(0, executed2);
            Assert.AreEqual(50, context.CorporateReputation);

            // Month 3 (Mar)
            int executed3 = scheduler.ProcessDueEffects(new GameDate(2026, 3), context);
            Assert.AreEqual(0, executed3);
            Assert.AreEqual(50, context.CorporateReputation);
            Assert.AreEqual(1, scheduler.PendingCount);
        }

        [Test]
        public void ProcessDueEffects_ExecutesExactlyOnDueDateAndCleansQueue()
        {
            var scheduler = new DelayedEffectScheduler();
            var context = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 50);

            var effect = ScriptableObject.CreateInstance<ReputationEffectSO>();
            effect.SetDeltaReputation(20);

            scheduler.ScheduleEffect(context.CurrentDate, 3, effect);

            // Month 4 (April) arrives -> Due!
            int executed = scheduler.ProcessDueEffects(new GameDate(2026, 4), context);

            Assert.AreEqual(1, executed);
            Assert.AreEqual(70, context.CorporateReputation); // 50 + 20
            Assert.AreEqual(0, scheduler.PendingCount);

            // Subsequent month (May) -> Not executed again!
            int executedMay = scheduler.ProcessDueEffects(new GameDate(2026, 5), context);
            Assert.AreEqual(0, executedMay);
            Assert.AreEqual(70, context.CorporateReputation);
        }

        [Test]
        public void ProcessDueEffects_MultipleEffects_ExecutesOnlyDueEntries()
        {
            var scheduler = new DelayedEffectScheduler();
            var context = new MockSimulationContext(Money.FromDollars(1000m), new GameDate(2026, 1), 50);

            var effShort = ScriptableObject.CreateInstance<ReputationEffectSO>();
            effShort.SetDeltaReputation(5);

            var effLong = ScriptableObject.CreateInstance<ReputationEffectSO>();
            effLong.SetDeltaReputation(15);

            scheduler.ScheduleEffect(context.CurrentDate, 1, effShort); // Due in Feb (Month 2)
            scheduler.ScheduleEffect(context.CurrentDate, 4, effLong);  // Due in May (Month 5)

            Assert.AreEqual(2, scheduler.PendingCount);

            // February arrives
            scheduler.ProcessDueEffects(new GameDate(2026, 2), context);

            Assert.AreEqual(55, context.CorporateReputation); // 50 + 5
            Assert.AreEqual(1, scheduler.PendingCount); // Long is still pending

            // May arrives
            scheduler.ProcessDueEffects(new GameDate(2026, 5), context);

            Assert.AreEqual(70, context.CorporateReputation); // 55 + 15
            Assert.AreEqual(0, scheduler.PendingCount);
        }
    }
}
