using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class BattleHudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI pendingText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI debugText;

        public void Refresh(int money, string locationName, int currentEnemyIndex, int totalEnemies, int pendingMultiplier, string stateLabel, string debugLabel)
        {
            if (moneyText != null)
            {
                moneyText.text = "$ " + money;
            }

            if (locationText != null)
            {
                locationText.text = locationName;
            }

            if (levelText != null)
            {
                levelText.text = "Level " + currentEnemyIndex + "/" + totalEnemies;
            }

            if (pendingText != null)
            {
                pendingText.text = pendingMultiplier > 1 ? "Next x" + pendingMultiplier : "Next -";
            }

            if (stateText != null)
            {
                stateText.text = stateLabel;
            }

            if (debugText != null)
            {
                debugText.text = debugLabel;
                debugText.gameObject.SetActive(!string.IsNullOrWhiteSpace(debugLabel));
            }
        }
    }
}
