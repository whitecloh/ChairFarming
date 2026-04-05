using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class BallTooltipView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;

        public void Show(BallDefinition ball)
        {
            if (ball == null)
            {
                Hide();
                return;
            }

            if (root != null)
            {
                root.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = ball.DisplayName;
            }

            if (bodyText != null)
            {
                bodyText.text = ball.Description;
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
