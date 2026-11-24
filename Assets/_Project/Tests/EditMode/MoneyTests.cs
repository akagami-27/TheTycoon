using System;
using NUnit.Framework;
using TheTycoon.Core;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class MoneyTests
    {
        [Test]
        public void ZeroMoney_HasZeroCentsAndDollars()
        {
            Money money = Money.Zero;
            Assert.AreEqual(0, money.Cents);
            Assert.AreEqual(0, money.Sen);
            Assert.AreEqual(0m, money.TotalDollars);
            Assert.AreEqual(0m, money.TotalMajorUnits);
            Assert.IsTrue(money.IsZero);
            Assert.IsFalse(money.IsPositive);
            Assert.IsFalse(money.IsNegative);
        }

        [Test]
        public void FromDollars_DecimalConversion_CorrectMinorUnits()
        {
            Money money = Money.FromDollars(123.45m);
            Assert.AreEqual(12345, money.Cents);
            Assert.AreEqual(12345, money.Sen);
            Assert.AreEqual(123.45m, money.TotalDollars);
            Assert.AreEqual(123.45m, money.TotalMajorUnits);
        }

        [Test]
        public void FromSen_MinorUnitsDirectInitialization()
        {
            Money money = Money.FromSen(25000000);
            Assert.AreEqual(25000000, money.Cents);
            Assert.AreEqual(250000.00m, money.TotalMajorUnits);
        }

        [Test]
        public void PositiveAndNegativeValues_PropertiesReflectSign()
        {
            Money positive = Money.FromDollars(50m);
            Money negative = Money.FromDollars(-50m);

            Assert.IsTrue(positive.IsPositive);
            Assert.IsFalse(positive.IsNegative);

            Assert.IsTrue(negative.IsNegative);
            Assert.IsFalse(negative.IsPositive);
        }

        [Test]
        public void AdditionAndSubtraction_ExactArithmetic()
        {
            Money a = Money.FromDollars(100.25m);
            Money b = Money.FromDollars(50.75m);

            Money sum = a + b;
            Money diff = a - b;

            Assert.AreEqual(15100, sum.Cents);
            Assert.AreEqual(151.00m, sum.TotalDollars);
            Assert.AreEqual(4950, diff.Cents);
            Assert.AreEqual(49.50m, diff.TotalDollars);
        }

        [Test]
        public void Multiplication_CalculatesExpectedResult()
        {
            Money basePrice = Money.FromDollars(10.00m);
            Money total = basePrice * 3;
            Assert.AreEqual(3000, total.Cents);
            Assert.AreEqual(30.00m, total.TotalDollars);

            Money interest = Money.FromDollars(100.00m) * 0.05m;
            Assert.AreEqual(500, interest.Cents);
            Assert.AreEqual(5.00m, interest.TotalDollars);
        }

        [Test]
        public void Division_CalculatesExactIntegerDivision()
        {
            Money amount = Money.FromDollars(10.00m);
            Money divided = amount / 2;
            Assert.AreEqual(500, divided.Cents);
        }

        [Test]
        public void DivisionByZero_ThrowsDivideByZeroException()
        {
            Money amount = Money.FromDollars(10.00m);
            Assert.Throws<DivideByZeroException>(() =>
            {
                var _ = amount / 0;
            });
        }

        [Test]
        public void DivisionOfMinValueByMinusOne_ThrowsOverflowException()
        {
            Money min = Money.MinValue;
            Assert.Throws<OverflowException>(() =>
            {
                var _ = min / -1;
            });
        }

        [Test]
        public void AdditionOverflow_ThrowsOverflowException()
        {
            Money max = Money.MaxValue;
            Money delta = Money.FromCents(1);

            Assert.Throws<OverflowException>(() =>
            {
                var _ = max + delta;
            });
        }

        [Test]
        public void SubtractionOverflow_ThrowsOverflowException()
        {
            Money min = Money.MinValue;
            Money delta = Money.FromCents(1);

            Assert.Throws<OverflowException>(() =>
            {
                var _ = min - delta;
            });
        }

        [Test]
        public void NegationOverflow_ThrowsOverflowException()
        {
            Money min = Money.MinValue;
            Assert.Throws<OverflowException>(() =>
            {
                var _ = -min;
            });
        }

        [Test]
        public void MultiplicationOverflow_ThrowsOverflowException()
        {
            Money huge = Money.FromCents(long.MaxValue / 2 + 10);
            Assert.Throws<OverflowException>(() =>
            {
                var _ = huge * 3;
            });

            Assert.Throws<OverflowException>(() =>
            {
                var _ = huge * 3.0m;
            });
        }

        [Test]
        public void FromDollars_OverflowingDecimal_ThrowsOverflowException()
        {
            decimal hugeDollars = (decimal)long.MaxValue;
            Assert.Throws<OverflowException>(() =>
            {
                Money.FromDollars(hugeDollars);
            });
        }

        [Test]
        public void ComparisonOperators_WorkCorrectly()
        {
            Money low = Money.FromDollars(100m);
            Money high = Money.FromDollars(200m);

            Assert.IsTrue(low < high);
            Assert.IsTrue(low <= high);
            Assert.IsTrue(high > low);
            Assert.IsTrue(high >= low);
            Assert.AreEqual(low, Money.FromDollars(100m));
            Assert.AreNotEqual(low, high);
        }

        [Test]
        public void FormattedString_MalaysianRinggitDefault()
        {
            Money sample = Money.FromSen(25000000);
            string formatted = sample.ToFormattedString();
            Assert.AreEqual("RM250,000.00", formatted);
        }

        [Test]
        public void FormattedString_NegativeAmount_PrefixesMinusSign()
        {
            Money negative = Money.FromDollars(-1500.50m);
            string formatted = negative.ToFormattedString();
            Assert.AreEqual("-RM1,500.50", formatted);
        }

        [Test]
        public void FormattedString_ConfigurableCurrencySymbol()
        {
            Money amount = Money.FromDollars(1000m);
            string usdFormatted = amount.ToFormattedString("$", includeDecimals: false);
            Assert.AreEqual("$1,000", usdFormatted);

            string gbpFormatted = amount.ToFormattedString("£", includeDecimals: true);
            Assert.AreEqual("£1,000.00", gbpFormatted);
        }
    }
}
