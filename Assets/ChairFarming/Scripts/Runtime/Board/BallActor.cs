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
            _spinSpeed = definition != null && definition.Category == BallCategory.Utility ? 420f : 300f;
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

                float distance = Vector2.Distance(previousPoint.Position, nextPoint.Position);
                float duration = Mathf.Max(0.035f, segmentBaseDuration * Mathf.Clamp(distance * 1.15f, 0.65f, 1.55f));

                yield return MoveSegment(previousPoint, nextPoint, duration);

                switch (nextPoint.PointType)
                {
                    case RoutePointType.PinImpact:
                        PlayedImpacts++;
                        onPinImpact?.Invoke(nextPoint.PinId);
                        yield return null;
                        break;

                    case RoutePointType.FingerLand:
                        onFingerLand?.Invoke(nextPoint.FingerIndex);
                        break;
                }
            }

            onCompleted?.Invoke(PlayedImpacts, plan.TargetFingerIndex);
        }

        private IEnumerator MoveSegment(RoutePoint fromPoint, RoutePoint toPoint, float duration)
        {
            Vector2 start = fromPoint.Position;
            Vector2 end = toPoint.Position;

            float elapsed = 0f;

            Vector2 direction = end - start;
            float horizontal = direction.x;
            float vertical = direction.y;

            float curveDepth = Mathf.Clamp(Mathf.Abs(horizontal) * 0.18f + Mathf.Abs(vertical) * 0.08f, 0.02f, 0.18f);

            Vector2 controlA = Vector2.Lerp(start, end, 0.33f);
            Vector2 controlB = Vector2.Lerp(start, end, 0.66f);

            if (end.y <= start.y)
            {
                controlA += new Vector2(horizontal * 0.10f, -curveDepth);
                controlB += new Vector2(horizontal * 0.04f, -curveDepth * 0.55f);
            }
            else
            {
                controlA += new Vector2(horizontal * 0.08f, curveDepth * 0.25f);
                controlB += new Vector2(horizontal * 0.02f, curveDepth * 0.10f);
            }

            Vector2 previousPosition = start;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float eased = t * t * (3f - 2f * t);
                Vector2 currentPosition = EvaluateCubicBezier(start, controlA, controlB, end, eased);

                transform.position = currentPosition;

                Vector2 frameVelocity = currentPosition - previousPosition;
                float signedRotation = Mathf.Sign(frameVelocity.x == 0f ? 1f : frameVelocity.x);
                transform.Rotate(0f, 0f, -signedRotation * _spinSpeed * Time.deltaTime);

                previousPosition = currentPosition;
                yield return null;
            }

            transform.position = end;
        }

        private static Vector2 EvaluateCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }
    }
}