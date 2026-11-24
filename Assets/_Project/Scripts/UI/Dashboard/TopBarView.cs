using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheTycoon.UI.Dashboard
{
    /// <summary>
    /// MonoBehaviour presentation component for the corporate top bar header.
    /// Handles user interaction for turn advancement and displays company identity and date.
    /// </summary>
    public class TopBarView : MonoBehaviour, ITopBarView
    {
        [Header("Text Labels")]
        [SerializeField] private TMP_Text _companyNameLabel;
        [SerializeField] private TMP_Text _dateLabel;
        [SerializeField] private TMP_Text _reputationLabel;

        [Header("Controls")]
        [SerializeField] private Button _advanceMonthButton;

        public event Action OnAdvanceClicked;

        private void Awake()
        {
            if (_advanceMonthButton != null)
            {
                _advanceMonthButton.onClick.AddListener(HandleAdvanceButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_advanceMonthButton != null)
            {
                _advanceMonthButton.onClick.RemoveListener(HandleAdvanceButtonClicked);
            }
        }

        public void SetCompanyName(string companyName)
        {
            if (_companyNameLabel != null)
            {
                _companyNameLabel.text = companyName ?? string.Empty;
            }
        }

        public void SetDate(string dateText)
        {
            if (_dateLabel != null)
            {
                _dateLabel.text = dateText ?? string.Empty;
            }
        }

        public void SetReputation(int reputation)
        {
            if (_reputationLabel != null)
            {
                _reputationLabel.text = $"Reputation: {reputation}%";
            }
        }

        public void SetAdvanceButtonInteractable(bool interactable)
        {
            if (_advanceMonthButton != null)
            {
                _advanceMonthButton.interactable = interactable;
            }
        }

        private void HandleAdvanceButtonClicked()
        {
            OnAdvanceClicked?.Invoke();
        }
    }
}
