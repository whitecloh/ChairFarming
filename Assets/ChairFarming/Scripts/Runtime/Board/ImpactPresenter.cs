using System.Collections;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class ImpactPresenter : MonoBehaviour
    {
        [SerializeField] private Transform fxRoot;
        [SerializeField] private SpriteRenderer flashPrefab;
        [SerializeField] private Color flashColor = new Color(1f, 0.95f, 0.45f, 0.9f);

        public void PresentPinImpact(BoardPinPoint pin)
        {
            if (pin == null)
            {
                return;
            }

            StartCoroutine(PulsePin(pin));
            SpawnFlash(pin.transform.position, 0.28f, 0.14f);
        }

        public void PresentFingerLanding(FingerSlotView finger)
        {
            if (finger == null)
            {
                return;
            }

            finger.PlayHit();
            SpawnFlash(finger.LandingPoint.position, 0.40f, 0.18f);
        }

        private IEnumerator PulsePin(BoardPinPoint pin)
        {
            SpriteRenderer renderer = pin.SpriteRenderer;
            if (renderer == null)
            {
                yield break;
            }

            Color baseColor = renderer.color;
            Vector3 baseScale = pin.transform.localScale;

            float duration = 0.08f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                renderer.color = Color.Lerp(flashColor, baseColor, t);
                pin.transform.localScale = Vector3.Lerp(baseScale * 1.08f, baseScale, t);

                yield return null;
            }

            renderer.color = baseColor;
            pin.transform.localScale = baseScale;
        }

        private void SpawnFlash(Vector3 position, float scale, float duration)
        {
            if (flashPrefab == null)
            {
                return;
            }

            Transform parent = fxRoot != null ? fxRoot : transform;
            SpriteRenderer flash = Instantiate(flashPrefab, position, Quaternion.identity, parent);
            flash.color = flashColor;
            flash.transform.localScale = Vector3.one * scale;

            StartCoroutine(FadeFlashRoutine(flash, duration));
        }

        private static IEnumerator FadeFlashRoutine(SpriteRenderer flash, float duration)
        {
            float elapsed = 0f;
            Color baseColor = flash.color;
            Vector3 baseScale = flash.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                flash.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0f, t));
                flash.transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.8f, t);

                yield return null;
            }

            if (flash != null)
            {
                Destroy(flash.gameObject);
            }
        }
    }
}
