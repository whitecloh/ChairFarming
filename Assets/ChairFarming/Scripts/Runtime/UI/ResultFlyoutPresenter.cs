using System.Collections;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class ResultFlyoutPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private FloatingDeltaTextView floatingTextPrefab;
        [SerializeField] private float travelDuration = 0.45f;
        [SerializeField] private Vector2 startScreenOffset = new Vector2(0f, 24f);

        public void PlayFromWorldToUi(Vector3 worldStart, RectTransform target, string text)
        {
            if (canvasRoot == null || floatingTextPrefab == null || target == null)
            {
                return;
            }

            Vector2 startAnchored;
            Vector2 targetAnchored;

            if (!TryWorldToCanvasPosition(worldStart, out startAnchored))
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRoot,
                    RectTransformUtility.WorldToScreenPoint(null, target.position),
                    null,
                    out targetAnchored))
            {
                return;
            }

            startAnchored += startScreenOffset;
            StartCoroutine(PlayRoutine(startAnchored, targetAnchored, text));
        }

        public void PlayFromUiToUi(RectTransform start, RectTransform target, string text)
        {
            if (canvasRoot == null || floatingTextPrefab == null || start == null || target == null)
            {
                return;
            }

            Vector2 startAnchored;
            Vector2 targetAnchored;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRoot,
                    RectTransformUtility.WorldToScreenPoint(null, start.position),
                    null,
                    out startAnchored))
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRoot,
                    RectTransformUtility.WorldToScreenPoint(null, target.position),
                    null,
                    out targetAnchored))
            {
                return;
            }

            StartCoroutine(PlayRoutine(startAnchored, targetAnchored, text));
        }

        private bool TryWorldToCanvasPosition(Vector3 worldPosition, out Vector2 canvasPosition)
        {
            Camera cam = worldCamera != null ? worldCamera : Camera.main;
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRoot,
                screenPoint,
                null,
                out canvasPosition);
        }

        private IEnumerator PlayRoutine(Vector2 start, Vector2 end, string text)
        {
            FloatingDeltaTextView instance = Instantiate(floatingTextPrefab, canvasRoot);
            instance.SetText(text);
            instance.SetAlpha(1f);
            instance.SetAnchoredPosition(start);

            float elapsed = 0f;

            while (elapsed < travelDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelDuration);
                float eased = EaseOutCubic(t);

                Vector2 current = Vector2.Lerp(start, end, eased);
                instance.SetAnchoredPosition(current);
                instance.SetAlpha(Mathf.Lerp(1f, 0f, t * 0.85f));

                yield return null;
            }

            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }

        private static float EaseOutCubic(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }
    }
}