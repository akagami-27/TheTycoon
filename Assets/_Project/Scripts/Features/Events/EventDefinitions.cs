using System;
using System.Collections.Generic;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    public enum EventCategory
    {
        Corporate,
        Financial,
        Workforce,
        Legal,
        Market,
        Executive
    }

    /// <summary>
    /// Contextual state interface passed to conditions and effects during event evaluation.
    /// Acts as a controlled facade: financial adjustments route directly to the authoritative FinancialLedger.
    /// </summary>
    public interface ISimulationContext
    {
        Money CurrentCash { get; }
        GameDate CurrentDate { get; }
        int CorporateReputation { get; }
        void ModifyCash(Money delta);
        void ModifyReputation(int delta);
    }

    /// <summary>
    /// Prerequisite condition for an event or decision choice.
    /// Compatible with future ScriptableObject-based condition definitions.
    /// </summary>
    public interface ICondition
    {
        bool Evaluate(ISimulationContext context);
        string GetFailureReason(ISimulationContext context);
    }

    /// <summary>
    /// Consequence executed upon choosing a decision option.
    /// Compatible with future ScriptableObject-based consequence definitions.
    /// </summary>
    public interface IEffect
    {
        void Apply(ISimulationContext context);
        string Description { get; }
    }

    /// <summary>
    /// Condition verifying a required minimum cash reserve.
    /// </summary>
    public class MinimumCashCondition : ICondition
    {
        public Money RequiredCash { get; }

        public MinimumCashCondition(Money requiredCash)
        {
            RequiredCash = requiredCash;
        }

        public bool Evaluate(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.CurrentCash >= RequiredCash;
        }

        public string GetFailureReason(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return $"Requires at least {RequiredCash.ToFormattedString()} (Current: {context.CurrentCash.ToFormattedString()})";
        }
    }

    /// <summary>
    /// Effect that applies an immediate cash adjustment directly via the simulation context.
    /// Does not maintain a secondary or independent cash balance.
    /// </summary>
    public class CashEffect : IEffect
    {
        public Money Delta { get; }

        public string Description => Delta.IsPositive 
            ? $"+{Delta.ToFormattedString()} Cash" 
            : $"{Delta.ToFormattedString()} Cash";

        public CashEffect(Money delta)
        {
            Delta = delta;
        }

        public void Apply(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.ModifyCash(Delta);
        }
    }

    /// <summary>
    /// Represents an option within an event that the player can choose.
    /// Encapsulates requirements checking and consequence enactment.
    /// </summary>
    public class ChoiceDefinition
    {
        public string ChoiceText { get; }
        public IReadOnlyList<ICondition> Requirements { get; }
        public IReadOnlyList<IEffect> Effects { get; }

        public ChoiceDefinition(string choiceText, IReadOnlyList<ICondition> requirements, IReadOnlyList<IEffect> effects)
        {
            if (string.IsNullOrWhiteSpace(choiceText))
            {
                throw new ArgumentException("Choice text cannot be null or whitespace.", nameof(choiceText));
            }

            ChoiceText = choiceText;
            Requirements = requirements ?? Array.Empty<ICondition>();
            Effects = effects ?? Array.Empty<IEffect>();
        }

        public bool CanSelect(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var req in Requirements)
            {
                if (req == null)
                {
                    continue;
                }
                if (!req.Evaluate(context))
                {
                    return false;
                }
            }
            return true;
        }

        public void Enact(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!CanSelect(context))
            {
                throw new InvalidOperationException("Cannot enact choice: requirements not met.");
            }

            foreach (var effect in Effects)
            {
                effect?.Apply(context);
            }
        }
    }
}
