using System;
using UnityEngine;
using TheTycoon.Core;

namespace TheTycoon.Features.Events
{
    /// <summary>
    /// Prerequisite condition restricting an event or choice by calendar date boundaries.
    /// </summary>
    [CreateAssetMenu(fileName = "Condition_Date", menuName = "The Tycoon/Conditions/Game Date")]
    public class DateConditionSO : PrerequisiteConditionSO
    {
        [SerializeField] private int _minYear = 2026;
        [Range(1, 12)]
        [SerializeField] private int _minMonth = 1;

        [SerializeField] private bool _hasMaxDate;
        [SerializeField] private int _maxYear = 2030;
        [Range(1, 12)]
        [SerializeField] private int _maxMonth = 12;

        public GameDate MinimumDate => new(_minYear, _minMonth);
        public GameDate? MaximumDate => _hasMaxDate ? new GameDate(_maxYear, _maxMonth) : null;

        public void SetMinimumDate(GameDate date)
        {
            _minYear = date.Year;
            _minMonth = date.Month;
        }

        public void SetMaximumDate(GameDate date)
        {
            _hasMaxDate = true;
            _maxYear = date.Year;
            _maxMonth = date.Month;
        }

        public void ClearMaximumDate()
        {
            _hasMaxDate = false;
        }

        public override bool Evaluate(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.CurrentDate < MinimumDate)
            {
                return false;
            }

            if (_hasMaxDate && context.CurrentDate > MaximumDate.Value)
            {
                return false;
            }

            return true;
        }

        public override string GetFailureReason(ISimulationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.CurrentDate < MinimumDate)
            {
                return $"Available from {MinimumDate.ToFormattedString()} (Current: {context.CurrentDate.ToFormattedString()})";
            }

            return $"Expired after {MaximumDate.Value.ToFormattedString()} (Current: {context.CurrentDate.ToFormattedString()})";
        }
    }
}
