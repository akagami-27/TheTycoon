using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TheTycoon.Features.Events;

namespace TheTycoon.UI.Events
{
    /// <summary>
    /// UI MonoBehaviour managing the Event Modal dialog presentation.
    /// Dynamically instantiates choice buttons and coordinates user decision input.
    /// </summary>
    public class EventModalView : MonoBehaviour, IEventModalView
    {
        [Header("Dialog Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Event Details")]
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _categoryLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;

        [Header("Choices")]
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;

        [Header("Status Message")]
        [SerializeField] private TextMeshProUGUI _statusLabel;

        private readonly List<ChoiceButtonView> _spawnedButtons = new();

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;
        public event Action<string> OnChoiceSelected;

        private void Awake()
        {
            if (_statusLabel != null)
            {
                _statusLabel.gameObject.SetActive(false);
            }
        }

        public void ShowEvent(EventDefinitionSO eventDef, IReadOnlyList<ChoiceViewModel> choices)
        {
            if (eventDef == null)
            {
                throw new ArgumentNullException(nameof(eventDef));
            }

            if (_titleLabel != null)
            {
                _titleLabel.text = eventDef.Title;
            }

            if (_categoryLabel != null)
            {
                _categoryLabel.text = eventDef.Category.ToString().ToUpperInvariant();
            }

            if (_descriptionLabel != null)
            {
                _descriptionLabel.text = eventDef.Description;
            }

            if (_statusLabel != null)
            {
                _statusLabel.text = string.Empty;
                _statusLabel.gameObject.SetActive(false);
            }

            // Clear previous choice buttons
            ClearSpawnedButtons();

            // Instantiate choice buttons
            if (_choicesContainer != null && _choiceButtonPrefab != null && choices != null)
            {
                foreach (var choiceModel in choices)
                {
                    GameObject buttonObj = Instantiate(_choiceButtonPrefab, _choicesContainer);
                    var choiceView = buttonObj.GetComponent<ChoiceButtonView>();
                    if (choiceView != null)
                    {
                        choiceView.Setup(choiceModel, HandleChoiceClicked);
                        _spawnedButtons.Add(choiceView);
                    }
                }
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
        }

        public void SetStatusMessage(string message, bool isError)
        {
            if (_statusLabel != null)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    _statusLabel.text = string.Empty;
                    _statusLabel.gameObject.SetActive(false);
                }
                else
                {
                    _statusLabel.text = isError ? $"<color=#EF4444>{message}</color>" : message;
                    _statusLabel.gameObject.SetActive(true);
                }
            }
        }

        public void SetChoicesInteractable(bool interactable)
        {
            foreach (var button in _spawnedButtons)
            {
                if (button != null)
                {
                    button.SetInteractable(interactable);
                }
            }
        }

        public void Hide()
        {
            ClearSpawnedButtons();
            if (_statusLabel != null)
            {
                _statusLabel.text = string.Empty;
                _statusLabel.gameObject.SetActive(false);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void HandleChoiceClicked(string choiceId)
        {
            OnChoiceSelected?.Invoke(choiceId);
        }

        private void ClearSpawnedButtons()
        {
            foreach (var button in _spawnedButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _spawnedButtons.Clear();
        }
    }
}
