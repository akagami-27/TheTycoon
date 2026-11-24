using System;
using NUnit.Framework;
using TheTycoon.Core;
using TheTycoon.Features.Company;

namespace TheTycoon.Tests.EditMode
{
    [TestFixture]
    public class CompanyProfileTests
    {
        [Test]
        public void ValidCompanyCreation_InitializesPropertiesCorrectly()
        {
            GameDate founding = new(2026, 1);
            Money valuation = Money.FromDollars(1000000m);
            CompanyProfile company = new("Acme Corporation", founding, valuation);

            Assert.AreEqual("Acme Corporation", company.CompanyName);
            Assert.AreEqual(founding, company.FoundingDate);
            Assert.AreEqual(valuation, company.Valuation);
            Assert.AreEqual(CompanyProfile.TotalBasisPoints, company.TotalAllocatedBasisPoints);
            Assert.AreEqual(0, company.UnallocatedBasisPoints);
            Assert.AreEqual(10000, company.GetShareholderBasisPoints("Founder"));
            Assert.AreEqual(100.00m, company.GetShareholderPercentage("Founder"));
        }

        [Test]
        public void InvalidCompanyName_ThrowsArgumentException()
        {
            GameDate founding = new(2026, 1);
            Money val = Money.FromDollars(100000m);

            Assert.Throws<ArgumentException>(() => new CompanyProfile(null, founding, val));
            Assert.Throws<ArgumentException>(() => new CompanyProfile("", founding, val));
            Assert.Throws<ArgumentException>(() => new CompanyProfile("   ", founding, val));
        }

        [Test]
        public void Rename_ValidAndInvalidInputs()
        {
            CompanyProfile company = new("Original Name", new GameDate(2026, 1), Money.FromDollars(100000m));

            company.Rename("New Corporate Name");
            Assert.AreEqual("New Corporate Name", company.CompanyName);

            Assert.Throws<ArgumentException>(() => company.Rename(""));
            Assert.Throws<ArgumentException>(() => company.Rename("   "));
            Assert.Throws<ArgumentException>(() => company.Rename(null));
        }

        [Test]
        public void NegativeValuation_ThrowsArgumentOutOfRangeException()
        {
            GameDate founding = new(2026, 1);
            Money negativeVal = Money.FromDollars(-100m);

            Assert.Throws<ArgumentOutOfRangeException>(() => new CompanyProfile("Acme", founding, negativeVal));

            CompanyProfile company = new("Acme", founding, Money.FromDollars(100m));
            Assert.Throws<ArgumentOutOfRangeException>(() => company.SetValuation(negativeVal));
        }

        [Test]
        public void CompanyValuation_UpdatesSuccessfully()
        {
            CompanyProfile company = new("Acme", new GameDate(2026, 1), Money.FromDollars(500000m));
            company.SetValuation(Money.FromDollars(1250000m));

            Assert.AreEqual(Money.FromDollars(1250000m), company.Valuation);
        }

        [Test]
        public void OwnershipAllocation_MultipleShareholders()
        {
            CompanyProfile company = new("Acme", new GameDate(2026, 1), Money.FromDollars(1000000m), 7000, "Founder");

            Assert.AreEqual(7000, company.TotalAllocatedBasisPoints);
            Assert.AreEqual(3000, company.UnallocatedBasisPoints);

            // Allocate to Angel Investor
            company.SetShareholderOwnership("Angel Investor", 2000);

            Assert.AreEqual(9000, company.TotalAllocatedBasisPoints);
            Assert.AreEqual(1000, company.UnallocatedBasisPoints);
            Assert.AreEqual(70.00m, company.GetShareholderPercentage("Founder"));
            Assert.AreEqual(20.00m, company.GetShareholderPercentage("Angel Investor"));

            // Allocate remaining to Employee Pool
            company.SetShareholderOwnership("Employee Pool", 1000);
            Assert.AreEqual(10000, company.TotalAllocatedBasisPoints);
            Assert.AreEqual(0, company.UnallocatedBasisPoints);
        }

        [Test]
        public void NegativeOwnership_ThrowsArgumentOutOfRangeException()
        {
            CompanyProfile company = new("Acme", new GameDate(2026, 1), Money.FromDollars(100000m), 5000);

            Assert.Throws<ArgumentOutOfRangeException>(() => company.SetShareholderOwnership("Investor", -500));
        }

        [Test]
        public void OwnershipOverflow_ThrowsInvalidOperationException()
        {
            CompanyProfile company = new("Acme", new GameDate(2026, 1), Money.FromDollars(100000m), 8000, "Founder");

            // Attempting to allocate 3000 bps when only 2000 bps are available
            Assert.Throws<InvalidOperationException>(() => company.SetShareholderOwnership("Venture Fund", 3000));
        }

        [Test]
        public void ZeroOwnershipAllocation_RemovesShareholder()
        {
            CompanyProfile company = new("Acme", new GameDate(2026, 1), Money.FromDollars(100000m), 8000, "Founder");
            company.SetShareholderOwnership("Investor", 2000);

            Assert.AreEqual(2000, company.GetShareholderBasisPoints("Investor"));

            // Setting to 0 removes allocation
            company.SetShareholderOwnership("Investor", 0);

            Assert.AreEqual(0, company.GetShareholderBasisPoints("Investor"));
            Assert.AreEqual(8000, company.TotalAllocatedBasisPoints);
            Assert.AreEqual(2000, company.UnallocatedBasisPoints);
        }
    }
}
