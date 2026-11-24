using System;
using TheTycoon.Core;

namespace TheTycoon.Features.Workforce
{
    /// <summary>
    /// Represents a corporate department managing a group of employees.
    /// Tracks headcount, average salary, productivity, and active status.
    /// All financial values use the Money type — no floating-point arithmetic.
    /// </summary>
    public class Department
    {
        /// <summary>Minimum productivity score (inclusive).</summary>
        public const int MinProductivity = 0;

        /// <summary>Maximum productivity score (inclusive).</summary>
        public const int MaxProductivity = 100;

        private int _employeeCount;
        private Money _averageMonthlySalary;
        private int _productivity;
        private bool _isActive;

        public DepartmentType Type { get; }
        public string Name { get; }

        /// <summary>Current number of employees in this department.</summary>
        public int EmployeeCount => _employeeCount;

        /// <summary>Average monthly salary per employee.</summary>
        public Money AverageMonthlySalary => _averageMonthlySalary;

        /// <summary>Total monthly payroll cost for this department (EmployeeCount × AverageMonthlySalary).</summary>
        public Money MonthlyPayroll => _employeeCount > 0 && _isActive
            ? _averageMonthlySalary * _employeeCount
            : Money.Zero;

        /// <summary>Productivity score in the range [0, 100]. Inactive departments report 0.</summary>
        public int Productivity => _isActive ? _productivity : 0;

        /// <summary>Whether the department is active and contributing to payroll and productivity.</summary>
        public bool IsActive => _isActive;

        public Department(
            DepartmentType type,
            string name,
            int initialEmployeeCount,
            Money averageMonthlySalary,
            int initialProductivity = 70,
            bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Department name cannot be null or whitespace.", nameof(name));
            }
            if (initialEmployeeCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialEmployeeCount),
                    "Employee count cannot be negative.");
            }
            if (averageMonthlySalary.IsNegative)
            {
                throw new ArgumentException("Average monthly salary cannot be negative.",
                    nameof(averageMonthlySalary));
            }
            if (initialProductivity < MinProductivity || initialProductivity > MaxProductivity)
            {
                throw new ArgumentOutOfRangeException(nameof(initialProductivity),
                    $"Productivity must be between {MinProductivity} and {MaxProductivity}.");
            }

            Type = type;
            Name = name;
            _employeeCount = initialEmployeeCount;
            _averageMonthlySalary = averageMonthlySalary;
            _productivity = initialProductivity;
            _isActive = isActive;
        }

        /// <summary>
        /// Increases the department headcount by the specified number of employees.
        /// </summary>
        /// <param name="count">Number of employees to add. Must be greater than zero.</param>
        public void Hire(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "Number of employees to hire must be greater than zero.");
            }

            checked
            {
                _employeeCount += count;
            }
        }

        /// <summary>
        /// Reduces the department headcount by the specified number.
        /// Cannot reduce below zero.
        /// </summary>
        /// <param name="count">Number of employees to remove. Must be greater than zero.</param>
        public void Reduce(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "Number of employees to reduce must be greater than zero.");
            }
            if (count > _employeeCount)
            {
                throw new InvalidOperationException(
                    $"Cannot reduce by {count}: department '{Name}' only has {_employeeCount} employees.");
            }

            _employeeCount -= count;
        }

        /// <summary>
        /// Updates the average monthly salary for this department.
        /// </summary>
        /// <param name="newSalary">New average monthly salary. Cannot be negative.</param>
        public void UpdateSalary(Money newSalary)
        {
            if (newSalary.IsNegative)
            {
                throw new ArgumentException("Average monthly salary cannot be negative.", nameof(newSalary));
            }

            _averageMonthlySalary = newSalary;
        }

        /// <summary>
        /// Sets the department's productivity score.
        /// </summary>
        /// <param name="productivity">Value between <see cref="MinProductivity"/> and <see cref="MaxProductivity"/>.</param>
        public void SetProductivity(int productivity)
        {
            if (productivity < MinProductivity || productivity > MaxProductivity)
            {
                throw new ArgumentOutOfRangeException(nameof(productivity),
                    $"Productivity must be between {MinProductivity} and {MaxProductivity}.");
            }

            _productivity = productivity;
        }

        /// <summary>Activates the department so it contributes to payroll and productivity.</summary>
        public void Activate()
        {
            _isActive = true;
        }

        /// <summary>
        /// Deactivates the department. An inactive department incurs no payroll
        /// and contributes zero productivity.
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
        }

        public override string ToString() =>
            $"{Name} [{Type}]: {_employeeCount} employees, " +
            $"Avg Salary {_averageMonthlySalary.ToFormattedString()}, " +
            $"Productivity {Productivity}, Active={_isActive}";
    }
}
