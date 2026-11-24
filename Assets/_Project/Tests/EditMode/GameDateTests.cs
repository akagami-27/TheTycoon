using System;
using NUnit.Framework;
using TheTycoon.Core;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class GameDateTests
    {
        [Test]
        public void Constructor_ValidInput_InitializesCorrectly()
        {
            GameDate date = new(2026, 1);
            Assert.AreEqual(2026, date.Year);
            Assert.AreEqual(1, date.Month);
            Assert.AreEqual("January", date.MonthName);
            Assert.AreEqual("Jan", date.ShortMonthName);
            Assert.AreEqual(1, date.Quarter);
            Assert.AreEqual("Jan 2026", date.ToFormattedString());
            Assert.AreEqual("January 2026 (Q1)", date.ToLongString());
        }

        [Test]
        public void Constructor_InvalidMonth_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameDate(2026, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameDate(2026, 13));
        }

        [Test]
        public void Constructor_InvalidYear_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameDate(0, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GameDate(-2026, 5));
        }

        [Test]
        public void AdvanceMonth_WithinYear_IncrementsMonth()
        {
            GameDate date = new(2026, 11);
            GameDate next = date.AdvanceMonth();

            Assert.AreEqual(2026, next.Year);
            Assert.AreEqual(12, next.Month);
            Assert.AreEqual("Dec 2026", next.ToFormattedString());
        }

        [Test]
        public void AdvanceMonth_AtDecember_RollsOverYear()
        {
            GameDate date = new(2026, 12);
            GameDate next = date.AdvanceMonth();

            Assert.AreEqual(2027, next.Year);
            Assert.AreEqual(1, next.Month);
            Assert.AreEqual("Jan 2027", next.ToFormattedString());
        }

        [Test]
        public void AdvanceMonth_ConsecutiveRoll_ProgressesAccurately()
        {
            GameDate date = new(2026, 11);
            GameDate d1 = date.AdvanceMonth();
            GameDate d2 = d1.AdvanceMonth();
            GameDate d3 = d2.AdvanceMonth();

            Assert.AreEqual(new GameDate(2026, 12), d1);
            Assert.AreEqual(new GameDate(2027, 1), d2);
            Assert.AreEqual(new GameDate(2027, 2), d3);
        }

        [Test]
        public void AdvanceMonths_TwelveMonths_AdvancesOneFullYear()
        {
            GameDate date = new(2026, 1);
            GameDate nextYear = date.AdvanceMonths(12);

            Assert.AreEqual(2027, nextYear.Year);
            Assert.AreEqual(1, nextYear.Month);
        }

        [Test]
        public void AdvanceMonths_Zero_ReturnsSameDate()
        {
            GameDate date = new(2026, 6);
            Assert.AreEqual(date, date.AdvanceMonths(0));
        }

        [Test]
        public void AdvanceMonths_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            GameDate date = new(2026, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => date.AdvanceMonths(-1));
        }

        [Test]
        public void AdvanceMonths_LargeCount_CalculatesCorrectYearAndMonth()
        {
            GameDate date = new(2026, 1);
            GameDate distantFuture = date.AdvanceMonths(120); // 10 years

            Assert.AreEqual(2036, distantFuture.Year);
            Assert.AreEqual(1, distantFuture.Month);
        }

        [Test]
        public void AdvanceMonths_ExtremeYearOverflow_ThrowsOverflowException()
        {
            GameDate date = new(int.MaxValue, 1);
            Assert.Throws<OverflowException>(() =>
            {
                date.AdvanceMonths(12);
            });
        }

        [Test]
        public void ComparisonOperators_OrderChronologically()
        {
            GameDate d1 = new(2026, 5);
            GameDate d2 = new(2026, 6);
            GameDate d3 = new(2027, 1);

            Assert.IsTrue(d1 < d2);
            Assert.IsTrue(d2 < d3);
            Assert.IsTrue(d3 > d1);
            Assert.AreEqual(d1, new GameDate(2026, 5));
            Assert.IsTrue(d1 <= new GameDate(2026, 5));
            Assert.IsTrue(d1 >= new GameDate(2026, 5));
        }
    }
}
