using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// UI component for an individual choice button within the Event Modal.
    /// Displays choice title, narrative context, requirement constraints, and consequence previews.
    /// </summary>
    public class ChoiceButtonView : MonoBehaviour, IChoiceButtonView
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _requirementsText;
        [SerializeField] private TextMeshProUGUI _consequencesText;
        [SerializeField] private Button _button;

        private string _choiceId;
        private Action<string> _onSelectedCallback;
        private bool _isChoiceEligible = true;

        public string ChoiceId => _choiceId;
        public bool IsChoiceEligible => _isChoiceEligible;
        public Button Button => _button;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        public void Setup(ChoiceViewModel model, Action<string> onSelected)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _choiceId = model.ChoiceId;
            _onSelectedCallback = onSelected;
            _isChoiceEligible = model.IsEnabled;

            if (_titleText != null)
            {
                _titleText.text = model.Title;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = model.Description;
                _descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(model.Description));
            }

            // Display requirements failure hint if not eligible
            if (_requirementsText != null)
            {
                if (!model.IsEnabled && !string.IsNullOrWhiteSpace(model.RequirementHint))
                {
                    _requirementsText.text = $"<color=#EF4444>Locked: {model.RequirementHint}</color>";
                    _requirementsText.gameObject.SetActive(true);
                }
                else
                {
                    _requirementsText.gameObject.SetActive(false);
                }
            }

            // Assemble consequence preview strings
            if (_consequencesText != null)
            {
                var sb = new StringBuilder();
                if (model.ImmediateConsequences.Count > 0)
                {
                    sb.Append("<color=#93C5FD>Immediate:</color> ");
                    sb.Append(string.Join(", ", model.ImmediateConsequences));
                }

                if (model.DelayedConsequences.Count > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("  |  ");
                    }
                    sb.Append("<color=#FDE047>Scheduled:</color> ");
                    sb.Append(string.Join(", ", model.DelayedConsequences));
                }

                _consequencesText.text = sb.ToString();
                _consequencesText.gameObject.SetActive(sb.Length > 0);
            }

            if (_button != null)
            {
                _button.interactable = model.IsEnabled;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                // Only allow interaction if choice itself is eligible
                _button.interactable = interactable && _isChoiceEligible;
            }
        }

        private void HandleButtonClicked()
        {
            if (_isChoiceEligible && !string.IsNullOrEmpty(_choiceId))
            {
                _onSelectedCallback?.Invoke(_choiceId);
            }
        }
    }
}
