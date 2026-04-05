using System.Collections;
using ChairFarming.Runtime.Battle;
using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class ResultPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI bodyText;

        private Coroutine _hideRoutine;

        public void Show(BallResolutionData data, float duration)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (bodyText != null && data != null)
            {
                bodyText.text =
                    "Hits: " + data.Collisions +
                    " | Finger #" + (data.FingerIndex + 1) + " x" + data.FingerMultiplier +
                    "\nMoney: " + FormatSigned(data.MoneyDelta) +
                    " | Damage: " + FormatSigned(data.DamageDelta) +
                    "\n" + data.Notes;
            }

            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideRoutine(duration));
        }

        public void HideImmediate()
        {
            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private IEnumerator HideRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (root != null)
            {
                root.SetActive(false);
            }

            _hideRoutine = null;
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }
    }
}
