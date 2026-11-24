using System;
using System.Collections.Generic;
using TheTycoon.Core;
using TheTycoon.Features.Finance;

namespace TheTycoon.Features.Workforce
{
    /// <summary>
    /// Manages all corporate departments and coordinates workforce operations.
    /// Authoritative source for headcount, payroll, and productivity data.
    ///
    /// Payroll integration:
    ///   Call <see cref="RecordPayrollExpense"/> once at the start of each monthly period
    ///   BEFORE <see cref="TheTycoon.Features.Time.TurnManager.AdvanceMonth"/> is called.
    ///   This records payroll as a ledger expense so it is included in the monthly settlement.
    ///   The caller (SimulationBootstrapper / TurnManager subscriber) is responsible for
    ///   ensuring this method is called exactly once per month.
    ///
    /// Duplicate-charge prevention:
    ///   <see cref="RecordPayrollExpense"/> tracks the last period for which payroll was
    ///   recorded and throws <see cref="InvalidOperationException"/> if called again for the
    ///   same period, ensuring no double-charging on re-entrant or erroneous calls.
    /// </summary>
    public class WorkforceManager
    {
        private const string PayrollCategory = "Payroll";
        private const string PayrollDescription = "Monthly Workforce Payroll";

        private readonly Dictionary<DepartmentType, Department> _departments = new();
        private GameDate? _lastPayrollPeriod;

        public IReadOnlyDictionary<DepartmentType, Department> Departments => _departments;

        /// <summary>
        /// Returns the department for the given type, or null if not registered.
        /// </summary>
        public Department GetDepartment(DepartmentType type)
        {
            _departments.TryGetValue(type, out var dept);
            return dept;
        }

        /// <summary>
        /// Registers a department with the workforce manager.
        /// Each <see cref="DepartmentType"/> may only be registered once.
        /// </summary>
        public void AddDepartment(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }
            if (_departments.ContainsKey(department.Type))
            {
                throw new InvalidOperationException(
                    $"A department of type '{department.Type}' is already registered.");
            }

            _departments[department.Type] = department;
        }

        // ─── Headcount Operations ────────────────────────────────────────────────

        /// <summary>Hires employees into the specified department.</summary>
        public void Hire(DepartmentType type, int count)
        {
            RequireDepartment(type).Hire(count);
        }

        /// <summary>Reduces headcount in the specified department through termination or attrition.</summary>
        public void Reduce(DepartmentType type, int count)
        {
            RequireDepartment(type).Reduce(count);
        }

        // ─── Salary Operations ───────────────────────────────────────────────────

        /// <summary>Updates the average monthly salary for a department.</summary>
        public void UpdateSalary(DepartmentType type, Money newSalary)
        {
            RequireDepartment(type).UpdateSalary(newSalary);
        }

        // ─── Productivity Operations ─────────────────────────────────────────────

        /// <summary>Sets a department's productivity score (0–100).</summary>
        public void SetProductivity(DepartmentType type, int productivity)
        {
            RequireDepartment(type).SetProductivity(productivity);
        }

        // ─── Activation ──────────────────────────────────────────────────────────

        /// <summary>Activates a department so it contributes to payroll and productivity.</summary>
        public void ActivateDepartment(DepartmentType type)
        {
            RequireDepartment(type).Activate();
        }

        /// <summary>
        /// Deactivates a department. Inactive departments incur no payroll
        /// and contribute zero productivity.
        /// </summary>
        public void DeactivateDepartment(DepartmentType type)
        {
            RequireDepartment(type).Deactivate();
        }

        // ─── Aggregated Metrics ──────────────────────────────────────────────────

        /// <summary>Total employee count across all active departments.</summary>
        public int TotalEmployeeCount()
        {
            int total = 0;
            foreach (var dept in _departments.Values)
            {
                if (dept.IsActive)
                {
                    total += dept.EmployeeCount;
                }
            }
            return total;
        }

        /// <summary>Combined monthly payroll of all active departments with employees.</summary>
        public Money TotalMonthlyPayroll()
        {
            Money total = Money.Zero;
            foreach (var dept in _departments.Values)
            {
                if (dept.IsActive && dept.EmployeeCount > 0)
                {
                    checked
                    {
                        total += dept.MonthlyPayroll;
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// Overall workforce productivity: weighted average by employee count
        /// across all active departments with employees.
        /// Returns 0 when no employees are present.
        /// </summary>
        public int OverallProductivity()
        {
            long weightedSum = 0;
            int totalEmployees = 0;

            foreach (var dept in _departments.Values)
            {
                if (!dept.IsActive || dept.EmployeeCount <= 0)
                {
                    continue;
                }

                checked
                {
                    weightedSum += (long)dept.Productivity * dept.EmployeeCount;
                    totalEmployees += dept.EmployeeCount;
                }
            }

            if (totalEmployees == 0)
            {
                return 0;
            }

            return (int)(weightedSum / totalEmployees);
        }

        /// <summary>Returns a point-in-time snapshot of the current workforce state.</summary>
        public WorkforceSummary GetSummary()
        {
            return new WorkforceSummary(
                TotalEmployeeCount(),
                TotalMonthlyPayroll(),
                OverallProductivity(),
                _departments.Count);
        }

        // ─── Payroll Integration ─────────────────────────────────────────────────

        /// <summary>
        /// Records the total payroll for all active departments with employees
        /// as a single expense entry in the provided <see cref="FinancialLedger"/>.
        ///
        /// Must be called exactly once per monthly period, before
        /// <see cref="TheTycoon.Features.Time.TurnManager.AdvanceMonth"/> settles accounts.
        ///
        /// Inactive departments and departments with zero employees are excluded.
        /// Zero-total payroll months produce no ledger entry.
        ///
        /// Throws <see cref="InvalidOperationException"/> if called twice for the same period.
        /// </summary>
        /// <param name="ledger">The authoritative financial ledger to charge.</param>
        /// <param name="currentPeriod">The game date of the period being settled.</param>
        public void RecordPayrollExpense(FinancialLedger ledger, GameDate currentPeriod)
        {
            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            if (_lastPayrollPeriod.HasValue && currentPeriod <= _lastPayrollPeriod.Value)
            {
                throw new InvalidOperationException(
                    $"Payroll for period {currentPeriod.ToFormattedString()} has already been recorded. " +
                    $"Last recorded period: {_lastPayrollPeriod.Value.ToFormattedString()}.");
            }

            Money payroll = TotalMonthlyPayroll();

            if (!payroll.IsZero)
            {
                ledger.AddExpense(PayrollCategory, PayrollDescription, payroll);
            }

            _lastPayrollPeriod = currentPeriod;
        }

        /// <summary>
        /// Returns true if payroll has already been recorded for the given period.
        /// </summary>
        public bool IsPayrollRecordedFor(GameDate period)
        {
            return _lastPayrollPeriod.HasValue && _lastPayrollPeriod.Value >= period;
        }

        // ─── Private Helpers ─────────────────────────────────────────────────────

        private Department RequireDepartment(DepartmentType type)
        {
            if (!_departments.TryGetValue(type, out var dept))
            {
                throw new InvalidOperationException(
                    $"No department of type '{type}' is registered in WorkforceManager.");
            }
            return dept;
        }
    }
}
