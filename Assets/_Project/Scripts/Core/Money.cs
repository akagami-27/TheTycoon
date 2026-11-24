using System;
using System.Globalization;

namespace TheTycoon.Core
{
    /// <summary>
    /// Immutable value type representing money with fixed-point integer minor units (sen/cents) precision.
    /// Prevents floating-point rounding inaccuracies in financial simulation logic and guarantees overflow safety.
    /// </summary>
    [Serializable]
    public readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        private readonly long _cents;

        /// <summary>
        /// Minor currency units (e.g. sen or cents). 100 minor units = 1 major unit.
        /// </summary>
        public long Cents => _cents;

        /// <summary>
        /// Alias for minor units in Malaysian Ringgit (sen).
        /// </summary>
        public long Sen => _cents;

        /// <summary>
        /// Decimal representation of total major units (e.g. Ringgit or Dollars).
        /// </summary>
        public decimal TotalMajorUnits => _cents / 100m;

        /// <summary>
        /// Backward-compatible alias for TotalMajorUnits.
        /// </summary>
        public decimal TotalDollars => TotalMajorUnits;

        public bool IsZero => _cents == 0;
        public bool IsPositive => _cents > 0;
        public bool IsNegative => _cents < 0;

        public static Money Zero => new(0);
        public static Money MinValue => new(long.MinValue);
        public static Money MaxValue => new(long.MaxValue);

        public Money(long cents)
        {
            _cents = cents;
        }

        public static Money FromCents(long cents) => new(cents);
        public static Money FromSen(long sen) => new(sen);

        public static Money FromDollars(long wholeDollars)
        {
            checked
            {
                return new Money(wholeDollars * 100);
            }
        }

        public static Money FromMajorUnits(long wholeUnits) => FromDollars(wholeUnits);

        public static Money FromDollars(decimal dollars)
        {
            decimal minorUnits = dollars * 100m;

            if (minorUnits > (decimal)long.MaxValue || minorUnits < (decimal)long.MinValue)
            {
                throw new OverflowException($"Value {dollars} overflows supported minor unit range.");
            }

            long cents = (long)Math.Round(minorUnits, MidpointRounding.AwayFromZero);
            return new Money(cents);
        }

        public static Money FromMajorUnits(decimal majorUnits) => FromDollars(majorUnits);

        /// <summary>
        /// Formats currency with a configurable symbol and decimal precision.
        /// Default symbol is Malaysian Ringgit (RM).
        /// </summary>
        public string ToFormattedString(string currencySymbol = "RM", bool includeDecimals = true, IFormatProvider provider = null)
        {
            IFormatProvider formatProvider = provider ?? CultureInfo.InvariantCulture;
            string numberPattern = includeDecimals ? "#,##0.00" : "#,##0";
            decimal absoluteMajorUnits = Math.Abs(TotalMajorUnits);
            string formattedNumber = absoluteMajorUnits.ToString(numberPattern, formatProvider);

            if (_cents < 0)
            {
                return $"-{currencySymbol}{formattedNumber}";
            }

            return $"{currencySymbol}{formattedNumber}";
        }

        public string ToDetailedString(string currencySymbol = "RM")
        {
            return ToFormattedString(currencySymbol, includeDecimals: true);
        }

        public static Money operator +(Money a, Money b)
        {
            checked
            {
                return new Money(a._cents + b._cents);
            }
        }

        public static Money operator -(Money a, Money b)
        {
            checked
            {
                return new Money(a._cents - b._cents);
            }
        }

        public static Money operator -(Money a)
        {
            checked
            {
                return new Money(-a._cents);
            }
        }

        public static Money operator *(Money a, int multiplier)
        {
            checked
            {
                return new Money(a._cents * multiplier);
            }
        }

        public static Money operator *(Money a, decimal multiplier)
        {
            decimal result = a._cents * multiplier;

            if (result > (decimal)long.MaxValue || result < (decimal)long.MinValue)
            {
                throw new OverflowException($"Multiplication resulted in overflow: {a._cents} * {multiplier}");
            }

            long cents = (long)Math.Round(result, MidpointRounding.AwayFromZero);
            return new Money(cents);
        }

        public static Money operator /(Money a, int divisor)
        {
            if (divisor == 0)
            {
                throw new DivideByZeroException("Cannot divide Money by zero.");
            }

            if (a._cents == long.MinValue && divisor == -1)
            {
                throw new OverflowException("Overflow dividing Money.MinValue by -1.");
            }

            return new Money(a._cents / divisor);
        }

        public static bool operator ==(Money a, Money b) => a._cents == b._cents;
        public static bool operator !=(Money a, Money b) => a._cents != b._cents;
        public static bool operator <(Money a, Money b) => a._cents < b._cents;
        public static bool operator <=(Money a, Money b) => a._cents <= b._cents;
        public static bool operator >(Money a, Money b) => a._cents > b._cents;
        public static bool operator >=(Money a, Money b) => a._cents >= b._cents;

        public bool Equals(Money other) => _cents == other._cents;
        public override bool Equals(object obj) => obj is Money other && Equals(other);
        public override int GetHashCode() => _cents.GetHashCode();
        public int CompareTo(Money other) => _cents.CompareTo(other._cents);
        public override string ToString() => ToFormattedString();
    }
}
