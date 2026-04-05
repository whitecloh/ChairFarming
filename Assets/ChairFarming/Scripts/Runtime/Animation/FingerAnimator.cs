using ChairFarming.Runtime.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.Animation
{
    public sealed class FingerAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Image imageTarget;
        [SerializeField] private FrameSpriteAnimator frameAnimator;

        private FingerAnimationDefinition _definition;

        public void ApplyDefinition(
            FingerAnimationDefinition definition,
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

        public void PlayHighlight()
        {
            if (_definition == null)
            {
                return;
            }

            if (_definition.Highlight != null &&
                _definition.Highlight.Frames != null &&
                _definition.Highlight.Frames.Length > 0)
            {
                frameAnimator.PlayOneShotThen(_definition.Highlight, _definition.Idle);
            }
            else
            {
                PlayIdle();
            }
        }
    }
}
