using System;
using System.Collections;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class BallActor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private BallDefinition _definition;
        private float _spinSpeed;

        public int PlayedImpacts { get; private set; }

        public void Initialize(BallDefinition definition)
        {
            _definition = definition;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = definition != null && definition.FlyingSprite != null
                    ? definition.FlyingSprite
                    : definition != null
                        ? definition.Icon
                        : null;
            }

            PlayedImpacts = 0;
            _spinSpeed = definition != null && definition.Category == BallCategory.Utility ? 360f : 240f;
        }

        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void PlayDrop(
            DropPlanData plan,
            float segmentBaseDuration,
            Action<int> onPinImpact,
            Action<int> onFingerLand,
            Action<int, int> onCompleted)
        {
            StopAllCoroutines();
            StartCoroutine(PlayDropRoutine(plan, segmentBaseDuration, onPinImpact, onFingerLand, onCompleted));
        }

        private IEnumerator PlayDropRoutine(
            DropPlanData plan,
            float segmentBaseDuration,
            Action<int> onPinImpact,
            Action<int> onFingerLand,
            Action<int, int> onCompleted)
        {
            if (plan == null || plan.Points == null || plan.Points.Count == 0)
            {
                onCompleted?.Invoke(0, 0);
                yield break;
            }

            transform.position = plan.Points[0].Position;
            PlayedImpacts = 0;

            for (int i = 1; i < plan.Points.Count; i++)
            {
                RoutePoint previousPoint = plan.Points[i - 1];
                RoutePoint nextPoint = plan.Points[i];

                float duration = segmentBaseDuration *
                                 Mathf.Clamp(Vector2.Distance(previousPoint.Position, nextPoint.Position) * 1.25f, 0.6f, 1.9f);

                yield return MoveSegment(previousPoint.Position, nextPoint.Position, duration);

                switch (nextPoint.PointType)
                {
                    case RoutePointType.PinImpact:
                        PlayedImpacts++;
                        onPinImpact?.Invoke(nextPoint.PinId);
                        break;

                    case RoutePointType.FingerLand:
                        onFingerLand?.Invoke(nextPoint.FingerIndex);
                        break;
                }
            }

            onCompleted?.Invoke(PlayedImpacts, plan.TargetFingerIndex);
        }

        private IEnumerator MoveSegment(Vector2 start, Vector2 end, float duration)
        {
            float elapsed = 0f;
            Vector2 delta = end - start;
            float amplitude = Mathf.Min(0.18f, delta.magnitude * 0.08f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);

                Vector2 linear = Vector2.Lerp(start, end, eased);
                Vector2 arc = Vector2.up * (Mathf.Sin(eased * Mathf.PI) * amplitude);

                transform.position = linear + arc;
                transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = end;
        }
    }
}
