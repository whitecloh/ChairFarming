using System;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class OfferCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image categoryBadgeImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private GameObject disabledOverlay;

        private BallDefinition _ball;
        private Action<BallDefinition> _onClicked;
        private Action<BallDefinition> _onHovered;
        private Action _onHoverExit;

        public void Bind(
            BallDefinition ball,
            bool affordable,
            bool interactable,
            Action<BallDefinition> onClicked,
            Action<BallDefinition> onHovered,
            Action onHoverExit)
        {
            _ball = ball;
            _onClicked = onClicked;
            _onHovered = onHovered;
            _onHoverExit = onHoverExit;

            gameObject.SetActive(ball != null);
            if (ball == null)
            {
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = ball.Icon;
            }

            if (titleText != null)
            {
                titleText.text = ball.DisplayName;
            }

            if (effectText != null)
            {
                effectText.text = ball.ShortEffectText;
            }

            if (costText != null)
            {
                costText.text = "$ " + ball.Cost;
            }

            Color categoryColor = GetCategoryColor(ball.Category);

            if (frameImage != null)
            {
                frameImage.color = affordable ? Color.white : new Color(0.45f, 0.45f, 0.48f, 1f);
            }

            if (categoryBadgeImage != null)
            {
                categoryBadgeImage.color = categoryColor;
            }

            if (disabledOverlay != null)
            {
                disabledOverlay.SetActive(!affordable || !interactable);
            }

            if (button != null)
            {
                button.interactable = interactable && affordable;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => _onClicked?.Invoke(_ball));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_ball != null)
            {
                _onHovered?.Invoke(_ball);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverExit?.Invoke();
        }

        private static Color GetCategoryColor(BallCategory category)
        {
            switch (category)
            {
                case BallCategory.Money:
                    return new Color(0.62f, 0.88f, 0.32f, 1f);

                case BallCategory.Damage:
                    return new Color(0.95f, 0.35f, 0.28f, 1f);

                case BallCategory.Utility:
                    return new Color(0.43f, 0.55f, 0.98f, 1f);

                default:
                    return Color.white;
            }
        }
    }
}
