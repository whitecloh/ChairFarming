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
        private GameBalanceConfig _balanceConfig;
        private float _spinSpeed;
        private Vector3 _baseScale = Vector3.one;

        public int PlayedImpacts { get; private set; }

        public void Initialize(BallDefinition definition, GameBalanceConfig balanceConfig)
        {
            _definition = definition;
            _balanceConfig = balanceConfig;

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
            _spinSpeed = definition != null && definition.Category == BallCategory.Utility
                ? _balanceConfig.UtilityBallSpinSpeed
                : _balanceConfig.BallSpinSpeed;

            _baseScale = transform.localScale;
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
            transform.localScale = _baseScale;
            PlayedImpacts = 0;

            for (int i = 1; i < plan.Points.Count; i++)
            {
                RoutePoint previousPoint = plan.Points[i - 1];
                RoutePoint nextPoint = plan.Points[i];

                switch (nextPoint.PointType)
                {
                    case RoutePointType.GatePass:
                        yield return PlayGateCrossSegment(previousPoint.Position, nextPoint.Position, segmentBaseDuration);
                        PlayedImpacts++;
                        onPinImpact?.Invoke(nextPoint.PinId);
                        break;

                    case RoutePointType.FingerLand:
                        yield return PlayFingerLanding(previousPoint.Position, nextPoint.Position, segmentBaseDuration);
                        onFingerLand?.Invoke(nextPoint.FingerIndex);
                        transform.localScale = _baseScale;
                        break;

                    default:
                        bool fromGate = previousPoint.PointType == RoutePointType.GatePass;
                        bool toFingerEntry = nextPoint.PointType == RoutePointType.FingerEntry;
                        bool lateFingerApproach = i < plan.Points.Count - 2 && plan.Points[i + 1].PointType == RoutePointType.FingerEntry;

                        if (fromGate)
                        {
                            yield return PlayPostGateFall(previousPoint.Position, nextPoint.Position, segmentBaseDuration);
                        }
                        else if (toFingerEntry || lateFingerApproach)
                        {
                            yield return PlayFingerApproach(previousPoint.Position, nextPoint.Position, segmentBaseDuration);
                        }
                        else
                        {
                            yield return PlayFallSegment(previousPoint.Position, nextPoint.Position, segmentBaseDuration);
                        }
                        break;
                }
            }

            onCompleted?.Invoke(PlayedImpacts, plan.TargetFingerIndex);
        }

        private IEnumerator PlayFallSegment(Vector2 start, Vector2 end, float segmentBaseDuration)
        {
            float distance = Vector2.Distance(start, end);
            float duration = Mathf.Max(0.055f, segmentBaseDuration * Mathf.Clamp(distance * 0.90f, 0.72f, 1.18f));
            float gravity = 13.0f;

            yield return PlayBallisticSegment(start, end, duration, gravity, SegmentPresentation.Fall);
        }

        private IEnumerator PlayGateCrossSegment(Vector2 start, Vector2 end, float segmentBaseDuration)
        {
            float distance = Vector2.Distance(start, end);
            float duration = Mathf.Max(0.042f, segmentBaseDuration * Mathf.Clamp(distance * 0.68f, 0.48f, 0.84f));

            Vector2 p1 = Vector2.Lerp(start, end, 0.34f);
            Vector2 p2 = Vector2.Lerp(start, end, 0.72f);

            float vertical = Mathf.Abs(end.y - start.y);
            float passDepth = Mathf.Clamp(0.02f + vertical * 0.02f, 0.012f, 0.045f);

            p1 += new Vector2((end.x - start.x) * 0.02f, -passDepth * 0.40f);
            p2 += new Vector2((end.x - start.x) * 0.01f, -passDepth);

            yield return PlayBezierSegment(start, p1, p2, end, duration, SegmentPresentation.GateCross);
        }

        private IEnumerator PlayPostGateFall(Vector2 start, Vector2 end, float segmentBaseDuration)
        {
            float distance = Vector2.Distance(start, end);
            float duration = Mathf.Max(0.050f, segmentBaseDuration * Mathf.Clamp(distance * 0.84f, 0.66f, 1.02f));
            float gravity = 14.0f;

            yield return PlayBallisticSegment(start, end, duration, gravity, SegmentPresentation.PostGateFall);
        }

        private IEnumerator PlayFingerApproach(Vector2 start, Vector2 end, float segmentBaseDuration)
        {
            float distance = Vector2.Distance(start, end);
            float duration = Mathf.Max(0.070f, segmentBaseDuration * Mathf.Clamp(distance * 0.92f, 0.76f, 1.10f));
            float gravity = 11.5f;

            yield return PlayBallisticSegment(start, end, duration, gravity, SegmentPresentation.FingerApproach);
        }

        private IEnumerator PlayFingerLanding(Vector2 start, Vector2 end, float segmentBaseDuration)
        {
            float distance = Vector2.Distance(start, end);
            float duration = Mathf.Max(0.080f, segmentBaseDuration * Mathf.Clamp(distance * 0.98f, 0.82f, 1.16f));
            float gravity = 10.0f;

            yield return PlayBallisticSegment(start, end, duration, gravity, SegmentPresentation.FingerLanding);
        }

        private IEnumerator PlayBallisticSegment(
            Vector2 start,
            Vector2 end,
            float duration,
            float gravity,
            SegmentPresentation presentation)
        {
            float elapsed = 0f;
            Vector2 previousPosition = start;

            float deltaX = end.x - start.x;
            float deltaY = end.y - start.y;

            float velocityX = deltaX / duration;
            float velocityY = (deltaY + 0.5f * gravity * duration * duration) / duration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp(elapsed, 0f, duration);
                float normalizedTime = Mathf.Clamp01(t / duration);

                float x = start.x + velocityX * t;
                float y = start.y + velocityY * t - 0.5f * gravity * t * t;

                Vector2 currentPosition = new Vector2(x, y);
                transform.position = currentPosition;

                ApplyPresentation(previousPosition, currentPosition, normalizedTime, presentation);
                previousPosition = currentPosition;
                yield return null;
            }

            transform.position = end;
            transform.localScale = _baseScale;
        }

        private IEnumerator PlayBezierSegment(
            Vector2 start,
            Vector2 p1,
            Vector2 p2,
            Vector2 end,
            float duration,
            SegmentPresentation presentation)
        {
            float elapsed = 0f;
            Vector2 previousPosition = start;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseInOutSmooth(t);

                Vector2 currentPosition = EvaluateCubicBezier(start, p1, p2, end, eased);
                transform.position = currentPosition;

                ApplyPresentation(previousPosition, currentPosition, t, presentation);
                previousPosition = currentPosition;
                yield return null;
            }

            transform.position = end;
            transform.localScale = _baseScale;
        }

        private void ApplyPresentation(Vector2 previousPosition, Vector2 currentPosition, float normalizedTime, SegmentPresentation presentation)
        {
            Vector2 delta = currentPosition - previousPosition;
            if (delta.sqrMagnitude <= 0.000001f)
            {
                transform.localScale = _baseScale;
                return;
            }

            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            float stretchBySpeed = Mathf.Clamp01(speed * 0.042f);

            float xScale = 1f;
            float yScale = 1f;

            switch (presentation)
            {
                case SegmentPresentation.Fall:
                    xScale = Mathf.Lerp(1.01f, 0.98f, stretchBySpeed);
                    yScale = Mathf.Lerp(0.995f, 1.05f, stretchBySpeed);
                    break;

                case SegmentPresentation.GateCross:
                    xScale = Mathf.Lerp(1.00f, 1.03f, normalizedTime * 0.6f);
                    yScale = Mathf.Lerp(1.00f, 0.97f, normalizedTime * 0.6f);
                    break;

                case SegmentPresentation.PostGateFall:
                    xScale = Mathf.Lerp(1.00f, 0.985f, stretchBySpeed);
                    yScale = Mathf.Lerp(1.00f, 1.04f, stretchBySpeed);
                    break;

                case SegmentPresentation.FingerApproach:
                    xScale = Mathf.Lerp(1.00f, 1.02f, normalizedTime * 0.35f);
                    yScale = Mathf.Lerp(1.00f, 0.98f, normalizedTime * 0.35f);
                    break;

                case SegmentPresentation.FingerLanding:
                    xScale = Mathf.Lerp(1.00f, 1.05f, normalizedTime);
                    yScale = Mathf.Lerp(1.00f, 0.93f, normalizedTime);
                    break;
            }

            transform.localScale = new Vector3(
                _baseScale.x * xScale,
                _baseScale.y * yScale,
                _baseScale.z);

            ApplyRotation(previousPosition, currentPosition, presentation);
        }

        private void ApplyRotation(Vector2 previousPosition, Vector2 currentPosition, SegmentPresentation presentation)
        {
            Vector2 frameVelocity = currentPosition - previousPosition;
            if (frameVelocity.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            float signedRotation = Mathf.Sign(frameVelocity.x == 0f ? 1f : frameVelocity.x);

            float boost = 1f;
            switch (presentation)
            {
                case SegmentPresentation.Fall:
                    boost = 0.88f;
                    break;
                case SegmentPresentation.GateCross:
                    boost = 0.92f;
                    break;
                case SegmentPresentation.PostGateFall:
                    boost = 0.86f;
                    break;
                case SegmentPresentation.FingerApproach:
                    boost = 0.82f;
                    break;
                case SegmentPresentation.FingerLanding:
                    boost = 0.68f;
                    break;
            }

            transform.Rotate(0f, 0f, -signedRotation * _spinSpeed * boost * Time.deltaTime);
        }

        private static float EaseInOutSmooth(float t)
        {
            return t * t * (3f - 2f * t);
        }

        private static Vector2 EvaluateCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        private enum SegmentPresentation
        {
            Fall = 0,
            GateCross = 1,
            PostGateFall = 2,
            FingerApproach = 3,
            FingerLanding = 4,
        }
    }
}