using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class FlightCounterView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private TextMeshProUGUI resolveText;

        private int _currentCount;

        public void Show()
        {
            _currentCount = 0;

            if (root != null)
            {
                root.SetActive(true);
            }

            if (resolveText != null)
            {
                resolveText.gameObject.SetActive(false);
            }

            RefreshCounter();
        }

        public void Increment()
        {
            _currentCount++;
            RefreshCounter();
        }

        public void ShowResolve(int multiplier)
        {
            if (resolveText == null)
            {
                return;
            }

            resolveText.gameObject.SetActive(true);
            resolveText.text = "x" + Mathf.Max(1, multiplier);
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void ResetImmediate()
        {
            _currentCount = 0;

            if (root != null)
            {
                root.SetActive(false);
            }

            if (resolveText != null)
            {
                resolveText.gameObject.SetActive(false);
            }
        }

        private void RefreshCounter()
        {
            if (counterText != null)
            {
                counterText.text = _currentCount.ToString();
            }
        }
    }
}