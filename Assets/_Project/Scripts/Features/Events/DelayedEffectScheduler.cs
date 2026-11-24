using System;
using System.Collections.Generic;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Represents an individual effect scheduled for future resolution.
    /// </summary>
    [Serializable]
    public sealed class ScheduledEffectEntry
    {
        public GameDate DueDate { get; }
        public IEffect Effect { get; }
        public string SourceEventId { get; }

        public ScheduledEffectEntry(GameDate dueDate, IEffect effect, string sourceEventId = null)
        {
            DueDate = dueDate;
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            SourceEventId = sourceEventId ?? string.Empty;
        }

        public override string ToString() => $"Due: {DueDate.ToFormattedString()} | {Effect.Description}";
    }

    /// <summary>
    /// Schedules and resolves delayed consequences across monthly turns.
    /// Completely turn-based and independent of real-time clocks or coroutines.
    /// </summary>
    public class DelayedEffectScheduler
    {
        private readonly List<ScheduledEffectEntry> _pendingEffects = new();

        public IReadOnlyList<ScheduledEffectEntry> PendingEffects => _pendingEffects;
        public int PendingCount => _pendingEffects.Count;

        /// <summary>
        /// Schedules an effect to execute precisely when the specified number of months has elapsed.
        /// DueDate = currentDate.AdvanceMonths(delayMonths).
        /// </summary>
        public ScheduledEffectEntry ScheduleEffect(
            GameDate currentDate,
            int delayMonths,
            IEffect effect,
            string sourceEventId = null)
        {
            if (delayMonths < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(delayMonths), "Delay months must be at least 1.");
            }
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            GameDate dueDate = currentDate.AdvanceMonths(delayMonths);
            var entry = new ScheduledEffectEntry(dueDate, effect, sourceEventId);
            _pendingEffects.Add(entry);
            return entry;
        }

        /// <summary>
        /// Processes and applies all effects whose due date has arrived (DueDate <= currentDate).
        /// Completed effects are removed from the pending queue. Future effects are preserved.
        /// </summary>
        public int ProcessDueEffects(GameDate currentDate, ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            int executedCount = 0;
            for (int i = _pendingEffects.Count - 1; i >= 0; i--)
            {
                var entry = _pendingEffects[i];
                if (entry.DueDate <= currentDate)
                {
                    try
                    {
                        entry.Effect.Apply(context);
                        executedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Remove from queue so faulty effect does not repeat indefinitely
                        _pendingEffects.RemoveAt(i);
                        throw new InvalidOperationException(
                            $"Failed to execute delayed effect from source '{entry.SourceEventId}' scheduled for {entry.DueDate.ToFormattedString()}.",
                            ex);
                    }

                    _pendingEffects.RemoveAt(i);
                }
            }

            return executedCount;
        }

        public void Clear()
        {
            _pendingEffects.Clear();
        }
    }
}
