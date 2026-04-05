using System;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class LostWindowView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI enemyNameText;
        [SerializeField] private Image enemyIconImage;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button exitToMenuButton;

        public void Show(EnemyDefinition enemy, Action onTryAgain, Action onExitToMenu)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (enemyNameText != null)
            {
                enemyNameText.text = enemy != null ? enemy.DisplayName : "Enemy";
            }

            if (enemyIconImage != null)
            {
                enemyIconImage.sprite = enemy != null && enemy.Icon != null ? enemy.Icon : enemy != null ? enemy.BodySprite : null;
            }

            if (tryAgainButton != null)
            {
                tryAgainButton.onClick.RemoveAllListeners();
                tryAgainButton.onClick.AddListener(() => onTryAgain?.Invoke());
            }

            if (exitToMenuButton != null)
            {
                exitToMenuButton.onClick.RemoveAllListeners();
                exitToMenuButton.onClick.AddListener(() => onExitToMenu?.Invoke());
            }
        }

        public void HideImmediate()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
