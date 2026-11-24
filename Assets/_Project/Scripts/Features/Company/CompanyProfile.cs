using System;
using System.Collections.Generic;
using TheTycoon.Core;

namespace TheTycoon.Features.Company
{
    /// <summary>
    /// Represents the corporate identity, founding date, share ownership distribution,
    /// and corporate valuation of the player's enterprise.
    /// Independent of Unity UI and does not store an independent cash balance.
    /// </summary>
    public class CompanyProfile
    {
        /// <summary>
        /// Total available equity in basis points (10,000 basis points = 100.00%).
        /// Eliminates floating-point rounding drift in corporate ownership calculations.
        /// </summary>
        public const int TotalBasisPoints = 10000;

        private readonly Dictionary<string, int> _shareholders = new(StringComparer.OrdinalIgnoreCase);

        public string CompanyName { get; private set; }
        public GameDate FoundingDate { get; }
        public Money Valuation { get; private set; }

        public IReadOnlyDictionary<string, int> ShareholderAllocations => _shareholders;

        public int TotalAllocatedBasisPoints
        {
            get
            {
                int total = 0;
                foreach (var kvp in _shareholders)
                {
                    total += kvp.Value;
                }
                return total;
            }
        }

        public int UnallocatedBasisPoints => TotalBasisPoints - TotalAllocatedBasisPoints;

        public CompanyProfile(
            string companyName,
            GameDate foundingDate,
            Money initialValuation,
            int founderBasisPoints = TotalBasisPoints,
            string founderName = "Founder")
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new ArgumentException("Company name cannot be null or whitespace.", nameof(companyName));
            }

            if (initialValuation.IsNegative)
            {
                throw new ArgumentOutOfRangeException(nameof(initialValuation), "Initial company valuation cannot be negative.");
            }

            if (founderBasisPoints < 0 || founderBasisPoints > TotalBasisPoints)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(founderBasisPoints),
                    $"Founder basis points must be between 0 and {TotalBasisPoints}.");
            }

            CompanyName = companyName;
            FoundingDate = foundingDate;
            Valuation = initialValuation;

            if (founderBasisPoints > 0)
            {
                string name = string.IsNullOrWhiteSpace(founderName) ? "Founder" : founderName;
                _shareholders[name] = founderBasisPoints;
            }
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Company name cannot be null or whitespace.", nameof(newName));
            }
            CompanyName = newName;
        }

        public void SetValuation(Money newValuation)
        {
            if (newValuation.IsNegative)
            {
                throw new ArgumentOutOfRangeException(nameof(newValuation), "Company valuation cannot be negative.");
            }
            Valuation = newValuation;
        }

        public void SetShareholderOwnership(string shareholderName, int basisPoints)
        {
            if (string.IsNullOrWhiteSpace(shareholderName))
            {
                throw new ArgumentException("Shareholder name cannot be null or whitespace.", nameof(shareholderName));
            }

            if (basisPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(basisPoints), "Ownership basis points cannot be negative.");
            }

            int existing = _shareholders.TryGetValue(shareholderName, out int current) ? current : 0;
            int newTotal = TotalAllocatedBasisPoints - existing + basisPoints;

            if (newTotal > TotalBasisPoints)
            {
                throw new InvalidOperationException(
                    $"Total ownership cannot exceed {TotalBasisPoints} basis points (100.00%). Attempted total: {newTotal} bps.");
            }

            if (basisPoints == 0)
            {
                _shareholders.Remove(shareholderName);
            }
            else
            {
                _shareholders[shareholderName] = basisPoints;
            }
        }

        public int GetShareholderBasisPoints(string shareholderName)
        {
            if (string.IsNullOrWhiteSpace(shareholderName))
            {
                return 0;
            }
            return _shareholders.TryGetValue(shareholderName, out int bps) ? bps : 0;
        }

        public decimal GetShareholderPercentage(string shareholderName)
        {
            return GetShareholderBasisPoints(shareholderName) / 100m;
        }
    }
}
