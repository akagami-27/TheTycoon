using System;

namespace TheTycoon.Core
{
    /// <summary>
    /// Represents the discrete monthly turn calendar date in the business simulation.
    /// Guarantees month rollover, overflow safety, and chronological comparison.
    /// </summary>
    [Serializable]
    public readonly struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        private static readonly string[] MonthNames = 
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        private static readonly string[] ShortMonthNames = 
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        public int Year { get; }
        public int Month { get; }

        public GameDate(int year, int month)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be greater than or equal to 1.");
            }
            if (month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
            }

            Year = year;
            Month = month;
        }

        public GameDate AdvanceMonth()
        {
            if (Month == 12)
            {
                checked
                {
                    return new GameDate(Year + 1, 1);
                }
            }
            return new GameDate(Year, Month + 1);
        }

        public GameDate AdvanceMonths(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Month increment count cannot be negative.");
            }

            if (count == 0)
            {
                return this;
            }

            checked
            {
                long totalMonths = ((long)Year * 12) + (Month - 1) + count;
                long nextYearLong = totalMonths / 12;
                int nextMonth = (int)((totalMonths % 12) + 1);

                if (nextYearLong > int.MaxValue)
                {
                    throw new OverflowException("Year overflowed maximum supported value.");
                }

                return new GameDate((int)nextYearLong, nextMonth);
            }
        }

        public int Quarter => ((Month - 1) / 3) + 1;

        public string MonthName => MonthNames[Month - 1];
        public string ShortMonthName => ShortMonthNames[Month - 1];

        public string ToFormattedString() => $"{ShortMonthName} {Year}";
        public string ToLongString() => $"{MonthName} {Year} (Q{Quarter})";

        public static bool operator ==(GameDate a, GameDate b) => a.Year == b.Year && a.Month == b.Month;
        public static bool operator !=(GameDate a, GameDate b) => !(a == b);
        public static bool operator <(GameDate a, GameDate b) => a.Year < b.Year || (a.Year == b.Year && a.Month < b.Month);
        public static bool operator <=(GameDate a, GameDate b) => a < b || a == b;
        public static bool operator >(GameDate a, GameDate b) => !(a <= b);
        public static bool operator >=(GameDate a, GameDate b) => !(a < b);

        public bool Equals(GameDate other) => Year == other.Year && Month == other.Month;
        public override bool Equals(object obj) => obj is GameDate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Year, Month);
        public int CompareTo(GameDate other)
        {
            int yearComparison = Year.CompareTo(other.Year);
            return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
        }
        public override string ToString() => ToFormattedString();
    }
}
