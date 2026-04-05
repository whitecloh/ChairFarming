using ChairFarming.Runtime.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.Animation
{
    public sealed class EnemyAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image imageTarget;
        [SerializeField] private FrameSpriteAnimator frameAnimator;

        private EnemyAnimationDefinition _definition;

        public void ApplyDefinition(
            EnemyAnimationDefinition definition,
            SpriteRenderer rendererOverride = null,
            Image imageOverride = null)
        {
            _definition = definition;

            if (rendererOverride != null)
            {
                spriteRenderer = rendererOverride;
            }

            if (imageOverride != null)
            {
                imageTarget = imageOverride;
            }

            if (frameAnimator == null)
            {
                frameAnimator = GetComponent<FrameSpriteAnimator>();
            }

            if (frameAnimator == null)
            {
                frameAnimator = gameObject.AddComponent<FrameSpriteAnimator>();
            }

            frameAnimator.SetTargets(spriteRenderer, imageTarget);
        }

        public void PlayIdle()
        {
            if (_definition != null)
            {
                frameAnimator.Play(_definition.Idle);
            }
        }

        public void PlayHit()
        {
            if (_definition == null)
            {
                return;
            }

            if (_definition.Hit != null && _definition.Hit.Frames != null && _definition.Hit.Frames.Length > 0)
            {
                frameAnimator.PlayOneShotThen(_definition.Hit, _definition.Idle);
            }
            else
            {
                PlayIdle();
            }
        }

        public void PlayDeath()
        {
            if (_definition == null)
            {
                return;
            }

            if (_definition.Death != null && _definition.Death.Frames != null && _definition.Death.Frames.Length > 0)
            {
                frameAnimator.PlayOneShotThen(_definition.Death, null);
            }
        }
    }
}
