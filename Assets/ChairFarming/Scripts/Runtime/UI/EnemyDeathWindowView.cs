using System;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class EnemyDeathWindowView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI enemyNameText;
        [SerializeField] private Image enemyIconImage;
        [SerializeField] private Button nextButton;

        public void Show(EnemyDefinition enemy, Action onNext)
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

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => onNext?.Invoke());
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
