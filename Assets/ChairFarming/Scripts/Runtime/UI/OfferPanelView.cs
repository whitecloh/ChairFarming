using System;
using System.Collections.Generic;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class OfferPanelView : MonoBehaviour
    {
        [SerializeField] private OfferCardView[] cards;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TextMeshProUGUI rerollCostText;
        [SerializeField] private Image rerollIconImage;

        private Action<BallDefinition> _onBallSelected;
        private Action _onRerollRequested;
        private Action<BallDefinition> _onOfferHovered;
        private Action _onOfferHoverExited;

        public void BindCallbacks(
            Action<BallDefinition> onBallSelected,
            Action onRerollRequested,
            Action<BallDefinition> onOfferHovered,
            Action onOfferHoverExited)
        {
            _onBallSelected = onBallSelected;
            _onRerollRequested = onRerollRequested;
            _onOfferHovered = onOfferHovered;
            _onOfferHoverExited = onOfferHoverExited;
        }

        public void Refresh(
            IReadOnlyList<BallDefinition> offers,
            int currentMoney,
            int rerollCost,
            bool interactable)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                BallDefinition ball = offers != null && i < offers.Count ? offers[i] : null;
                bool affordable = ball != null && currentMoney >= ball.Cost;

                cards[i].Bind(
                    ball,
                    affordable,
                    interactable,
                    _onBallSelected,
                    _onOfferHovered,
                    _onOfferHoverExited);
            }

            if (rerollCostText != null)
            {
                rerollCostText.text = "$ " + rerollCost;
            }

            if (rerollButton != null)
            {
                rerollButton.interactable = interactable && currentMoney >= rerollCost;
                rerollButton.onClick.RemoveAllListeners();
                rerollButton.onClick.AddListener(() => _onRerollRequested?.Invoke());
            }
        }

        public void ApplyLocation(LocationDefinition location)
        {
            if (location != null &&
                location.Theme != null &&
                location.Theme.RerollButtonIcon != null &&
                rerollIconImage != null)
            {
                rerollIconImage.sprite = location.Theme.RerollButtonIcon;
            }
        }
    }
}
