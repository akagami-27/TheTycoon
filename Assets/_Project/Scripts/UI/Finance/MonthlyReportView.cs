using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TheTycoon.Features.Finance;

namespace TheTycoon.UI.Finance
{
    /// <summary>
    /// MonoBehaviour presentation modal displaying a completed monthly financial settlement report.
    /// </summary>
    public class MonthlyReportView : MonoBehaviour, IMonthlyReportView
    {
        [Header("Report Panel Root")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Report Details")]
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _openingCashLabel;
        [SerializeField] private TMP_Text _revenueLabel;
        [SerializeField] private TMP_Text _expensesLabel;
        [SerializeField] private TMP_Text _netProfitLabel;
        [SerializeField] private TMP_Text _closingCashLabel;

        [Header("Controls")]
        [SerializeField] private Button _dismissButton;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public event Action OnDismissClicked;

        private void Awake()
        {
            if (_dismissButton != null)
            {
                _dismissButton.onClick.AddListener(HandleDismissButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_dismissButton != null)
            {
                _dismissButton.onClick.RemoveListener(HandleDismissButtonClicked);
            }
        }

        public void ShowReport(MonthlyFinancialSummary summary)
        {
            if (summary == null)
            {
                return;
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = $"{summary.Period.MonthName} {summary.Period.Year} Financial Report";
            }

            if (_openingCashLabel != null)
            {
                _openingCashLabel.text = $"Opening Balance: {summary.OpeningCash.ToFormattedString()}";
            }

            if (_revenueLabel != null)
            {
                _revenueLabel.text = $"Total Revenue: {summary.TotalRevenue.ToFormattedString()}";
            }

            if (_expensesLabel != null)
            {
                _expensesLabel.text = $"Total Expenses: {summary.TotalExpenses.ToFormattedString()}";
            }

            if (_netProfitLabel != null)
            {
                _netProfitLabel.text = $"Net Profit / Loss: {summary.NetProfit.ToFormattedString()}";
                _netProfitLabel.color = summary.IsProfitable 
                    ? new Color(0.2f, 0.8f, 0.3f) 
                    : (summary.IsLoss ? new Color(0.9f, 0.25f, 0.25f) : Color.white);
            }

            if (_closingCashLabel != null)
            {
                _closingCashLabel.text = $"Closing Balance: {summary.ClosingCash.ToFormattedString()}";
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
        }

        public void HideReport()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void HandleDismissButtonClicked()
        {
            OnDismissClicked?.Invoke();
        }
    }
}
