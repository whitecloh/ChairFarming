using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class BattleLogEntryView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI bodyText;

        public void SetText(string text)
        {
            if (bodyText != null)
            {
                bodyText.text = text;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
    }
}