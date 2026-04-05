using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class FloatingDeltaTextView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI valueText;

        public RectTransform RectTransform => rectTransform;

        public void SetText(string text)
        {
            if (valueText != null)
            {
                valueText.text = text;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        public void SetAnchoredPosition(Vector2 position)
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}