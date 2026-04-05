using ChairFarming.Runtime.Animation;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class EnemyView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image bodyImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private EnemyAnimator enemyAnimator;

        private EnemyDefinition _currentEnemy;

        public void ApplyEnemy(EnemyDefinition enemy)
        {
            _currentEnemy = enemy;

            if (_currentEnemy == null)
            {
                return;
            }

            if (bodyImage != null)
            {
                bodyImage.sprite = enemy.BodySprite;
            }

            if (iconImage != null)
            {
                iconImage.sprite = enemy.Icon != null ? enemy.Icon : enemy.BodySprite;
            }

            if (nameText != null)
            {
                nameText.text = enemy.DisplayName;
            }

            if (enemyAnimator != null)
            {
                enemyAnimator.ApplyDefinition(enemy.AnimationDefinition, null, bodyImage);
                enemyAnimator.PlayIdle();
            }
        }

        public void RefreshHp(EnemyDefinition enemy, int currentHp)
        {
            if (enemy == null)
            {
                return;
            }

            float fill = enemy.MaxHp > 0 ? Mathf.Clamp01(currentHp / (float)enemy.MaxHp) : 0f;

            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = fill;
            }

            if (hpText != null)
            {
                hpText.text = currentHp + " / " + enemy.MaxHp;
            }
        }

        public void PlayHitOrDeath(bool isDead)
        {
            if (enemyAnimator == null)
            {
                return;
            }

            if (isDead)
            {
                enemyAnimator.PlayDeath();
            }
            else
            {
                enemyAnimator.PlayHit();
            }
        }
    }
}
