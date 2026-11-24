using System;
using TheTycoon.Core;

namespace TheTycoon.Features.Workforce
{
    /// <summary>
    /// Immutable snapshot of the workforce state at a point in time.
    /// Returned by WorkforceManager to summarise current headcount, payroll and productivity.
    /// </summary>
    public sealed class WorkforceSummary
    {
        /// <summary>Total number of active employees across all active departments.</summary>
        public int TotalEmployeeCount { get; }

        /// <summary>Combined monthly payroll cost of all active departments.</summary>
        public Money TotalMonthlyPayroll { get; }

        /// <summary>
        /// Weighted average productivity across all active departments.
        /// Weighted by employee count per department.
        /// Returns 0 when there are no employees.
        /// </summary>
        public int OverallProductivity { get; }

        /// <summary>Number of departments included in this summary.</summary>
        public int DepartmentCount { get; }

        public WorkforceSummary(
            int totalEmployeeCount,
            Money totalMonthlyPayroll,
            int overallProductivity,
            int departmentCount)
        {
            if (totalEmployeeCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalEmployeeCount));
            }
            if (totalMonthlyPayroll.IsNegative)
            {
                throw new ArgumentException("Total payroll cannot be negative.", nameof(totalMonthlyPayroll));
            }
            if (overallProductivity < 0 || overallProductivity > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(overallProductivity),
                    "Overall productivity must be between 0 and 100.");
            }
            if (departmentCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(departmentCount));
            }

            TotalEmployeeCount = totalEmployeeCount;
            TotalMonthlyPayroll = totalMonthlyPayroll;
            OverallProductivity = overallProductivity;
            DepartmentCount = departmentCount;
        }

        public override string ToString() =>
            $"Workforce: {TotalEmployeeCount} employees, " +
            $"Payroll {TotalMonthlyPayroll.ToFormattedString()}/mo, " +
            $"Productivity {OverallProductivity}/100";
    }
}
