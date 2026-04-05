using System.Collections;
using ChairFarming.Runtime.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.Animation
{
    public sealed class FrameSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private Image targetImage;

        private Coroutine _playRoutine;

        public void SetTargets(SpriteRenderer spriteRenderer, Image image = null)
        {
            targetSpriteRenderer = spriteRenderer;
            targetImage = image;
        }

        public void SetTargetRenderer(SpriteRenderer spriteRenderer)
        {
            targetSpriteRenderer = spriteRenderer;
            targetImage = null;
        }

        public void SetTargetImage(Image image)
        {
            targetImage = image;
            targetSpriteRenderer = null;
        }

        public void Play(FrameAnimationClip clip)
        {
            if (!CanPlay(clip))
            {
                return;
            }

            StopCurrentRoutine();
            _playRoutine = StartCoroutine(PlayLoopingRoutine(clip));
        }

        public void PlayOneShotThen(FrameAnimationClip oneShot, FrameAnimationClip fallbackLoop)
        {
            if (!CanPlay(oneShot))
            {
                if (fallbackLoop != null)
                {
                    Play(fallbackLoop);
                }

                return;
            }

            StopCurrentRoutine();
            _playRoutine = StartCoroutine(PlayOneShotThenRoutine(oneShot, fallbackLoop));
        }

        public void StopAndShowFirst(FrameAnimationClip clip)
        {
            StopCurrentRoutine();

            if (!CanPlay(clip))
            {
                return;
            }

            ApplySprite(clip.Frames[0]);
        }

        private bool CanPlay(FrameAnimationClip clip)
        {
            return clip != null &&
                   clip.Frames != null &&
                   clip.Frames.Length > 0 &&
                   (targetSpriteRenderer != null || targetImage != null);
        }

        private void StopCurrentRoutine()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
        }

        private IEnumerator PlayLoopingRoutine(FrameAnimationClip clip)
        {
            if (clip.Loop)
            {
                while (true)
                {
                    yield return PlayFramesRoutine(clip);
                }
            }
            else
            {
                yield return PlayFramesRoutine(clip);
                _playRoutine = null;
            }
        }

        private IEnumerator PlayOneShotThenRoutine(FrameAnimationClip oneShot, FrameAnimationClip fallbackLoop)
        {
            yield return PlayFramesRoutine(oneShot);

            if (fallbackLoop != null)
            {
                _playRoutine = StartCoroutine(PlayLoopingRoutine(fallbackLoop));
                yield break;
            }

            _playRoutine = null;
        }

        private IEnumerator PlayFramesRoutine(FrameAnimationClip clip)
        {
            for (int i = 0; i < clip.Frames.Length; i++)
            {
                ApplySprite(clip.Frames[i]);
                yield return new WaitForSeconds(clip.FrameDuration);
            }
        }

        private void ApplySprite(Sprite sprite)
        {
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sprite = sprite;
            }

            if (targetImage != null)
            {
                targetImage.sprite = sprite;
            }
        }

        private void OnDisable()
        {
            StopCurrentRoutine();
        }
    }
}
