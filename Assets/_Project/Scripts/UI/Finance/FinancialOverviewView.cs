using TMPro;
using UnityEngine;

namespace TheTycoon.UI.Finance
{
    /// <summary>
    /// MonoBehaviour presentation component for the financial overview dashboard card.
    /// Displays current corporate cash and previous month's operational performance metrics.
    /// </summary>
    public class FinancialOverviewView : MonoBehaviour, IFinancialOverviewView
    {
        [Header("Financial Display Labels")]
        [SerializeField] private TMP_Text _cashLabel;
        [SerializeField] private TMP_Text _revenueLabel;
        [SerializeField] private TMP_Text _expensesLabel;
        [SerializeField] private TMP_Text _netProfitLabel;

        public void SetCashBalance(string cashText)
        {
            if (_cashLabel != null)
            {
                _cashLabel.text = cashText ?? string.Empty;
            }
        }

        public void SetPreviousMonthMetrics(
            string revenueText,
            string expenseText,
            string netProfitText,
            bool isAvailable,
            bool isProfitable)
        {
            if (_revenueLabel != null)
            {
                _revenueLabel.text = $"Revenue: {revenueText}";
            }

            if (_expensesLabel != null)
            {
                _expensesLabel.text = $"Expenses: {expenseText}";
            }

            if (_netProfitLabel != null)
            {
                _netProfitLabel.text = $"Net: {netProfitText}";
                // Color highlight: Green for profit, red for loss, neutral if not available
                if (!isAvailable)
                {
                    _netProfitLabel.color = Color.gray;
                }
                else
                {
                    _netProfitLabel.color = isProfitable ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.9f, 0.25f, 0.25f);
                }
            }
        }
    }
}
