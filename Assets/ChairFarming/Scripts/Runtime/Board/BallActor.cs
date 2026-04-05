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

                bool isImpactApproach = nextPoint.PointType == RoutePointType.PinImpact;
                bool isImpactRebound = previousPoint.PointType == RoutePointType.PinImpact;
                bool isFingerLanding = nextPoint.PointType == RoutePointType.FingerLand;

                float distance = Vector2.Distance(previousPoint.Position, nextPoint.Position);
                float durationMultiplier = GetDurationMultiplier(previousPoint.PointType, nextPoint.PointType);
                float duration = Mathf.Max(0.04f, segmentBaseDuration * Mathf.Clamp(distance * durationMultiplier, 0.7f, 2.0f));

                if (isImpactRebound)
                {
                    yield return MoveBounceSegment(previousPoint.Position, nextPoint.Position, duration);
                }
                else
                {
                    yield return MoveStandardSegment(previousPoint.Position, nextPoint.Position, duration, isImpactApproach, isFingerLanding);
                }

                switch (nextPoint.PointType)
                {
                    case RoutePointType.PinImpact:
                        PlayedImpacts++;
                        onPinImpact?.Invoke(nextPoint.PinId);

                        yield return PlayImpactSquash();

                        if (_balanceConfig != null && _balanceConfig.ImpactPause > 0f)
                        {
                            yield return new WaitForSeconds(_balanceConfig.ImpactPause);
                        }
                        break;

                    case RoutePointType.FingerLand:
                        onFingerLand?.Invoke(nextPoint.FingerIndex);
                        transform.localScale = _baseScale;
                        break;
                }
            }

            onCompleted?.Invoke(PlayedImpacts, plan.TargetFingerIndex);
        }

        private float GetDurationMultiplier(RoutePointType fromType, RoutePointType toType)
        {
            if (_balanceConfig == null)
            {
                return 1f;
            }

            if (toType == RoutePointType.PinImpact)
            {
                return _balanceConfig.ImpactApproachDurationMultiplier;
            }

            if (fromType == RoutePointType.PinImpact)
            {
                return _balanceConfig.ImpactReboundDurationMultiplier;
            }

            if (toType == RoutePointType.FingerLand)
            {
                return _balanceConfig.FingerLandingDurationMultiplier;
            }

            return _balanceConfig.NormalMoveDurationMultiplier;
        }

        private IEnumerator MoveStandardSegment(Vector2 start, Vector2 end, float duration, bool isImpactApproach, bool isFingerLanding)
        {
            Vector2 p1;
            Vector2 p2;
            BuildStandardControlPoints(start, end, isImpactApproach, isFingerLanding, out p1, out p2);

            float elapsed = 0f;
            Vector2 previousPosition = start;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseInOutSmooth(t);

                Vector2 currentPosition = EvaluateCubicBezier(start, p1, p2, end, eased);
                transform.position = currentPosition;

                ApplyRotation(previousPosition, currentPosition, 0.9f);
                previousPosition = currentPosition;
                yield return null;
            }

            transform.position = end;
        }

        private IEnumerator MoveBounceSegment(Vector2 start, Vector2 end, float duration)
        {
            float elapsed = 0f;
            Vector2 previousPosition = start;

            float arcHeight = Mathf.Max(0.08f, Mathf.Min(0.22f, Mathf.Abs(end.x - start.x) * 0.35f + 0.10f));

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float x = Mathf.Lerp(start.x, end.x, EaseOutQuad(t));
                float linearY = Mathf.Lerp(start.y, end.y, t);
                float arcY = Mathf.Sin(t * Mathf.PI) * arcHeight;

                Vector2 currentPosition = new Vector2(x, linearY + arcY);
                transform.position = currentPosition;

                float stretch = Mathf.Lerp(1.03f, 0.99f, t);
                transform.localScale = new Vector3(
                    _baseScale.x * (2f - stretch),
                    _baseScale.y * stretch,
                    _baseScale.z);

                ApplyRotation(previousPosition, currentPosition, 1.0f);
                previousPosition = currentPosition;
                yield return null;
            }

            transform.position = end;
            transform.localScale = _baseScale;
        }

        private IEnumerator PlayImpactSquash()
        {
            if (_balanceConfig == null)
            {
                yield break;
            }

            float duration = Mathf.Max(0.001f, _balanceConfig.ImpactSquashDuration * 0.65f);
            Vector3 squashScale = new Vector3(
                _baseScale.x * Mathf.Lerp(1f, _balanceConfig.ImpactSquashX, 0.65f),
                _baseScale.y * Mathf.Lerp(1f, _balanceConfig.ImpactSquashY, 0.65f),
                _baseScale.z);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(_baseScale, squashScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(squashScale, _baseScale, t);
                yield return null;
            }

            transform.localScale = _baseScale;
        }

        private void ApplyRotation(Vector2 previousPosition, Vector2 currentPosition, float boost)
        {
            Vector2 frameVelocity = currentPosition - previousPosition;
            if (frameVelocity.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            float signedRotation = Mathf.Sign(frameVelocity.x == 0f ? 1f : frameVelocity.x);
            transform.Rotate(0f, 0f, -signedRotation * _spinSpeed * boost * Time.deltaTime);
        }

        private void BuildStandardControlPoints(
            Vector2 start,
            Vector2 end,
            bool isImpactApproach,
            bool isFingerLanding,
            out Vector2 p1,
            out Vector2 p2)
        {
            Vector2 delta = end - start;
            float vertical = Mathf.Abs(delta.y);

            p1 = Vector2.Lerp(start, end, 0.33f);
            p2 = Vector2.Lerp(start, end, 0.66f);

            if (isImpactApproach)
            {
                float dropDepth = Mathf.Clamp(0.05f + vertical * 0.05f, 0.03f, 0.10f);
                p1 += new Vector2(delta.x * 0.02f, -dropDepth);
                p2 += new Vector2(delta.x * 0.005f, -dropDepth * 0.15f);
                return;
            }

            if (isFingerLanding)
            {
                float settle = Mathf.Clamp(0.04f + vertical * 0.04f, 0.02f, 0.08f);
                p1 += new Vector2(delta.x * 0.015f, -settle * 0.05f);
                p2 += new Vector2(delta.x * 0.005f, -settle);
                return;
            }

            float softDrop = Mathf.Clamp(0.03f + vertical * 0.04f, 0.02f, 0.08f);
            p1 += new Vector2(delta.x * 0.015f, -softDrop);
            p2 += new Vector2(delta.x * 0.005f, -softDrop * 0.15f);
        }

        private static float EaseInOutSmooth(float t)
        {
            return t * t * (3f - 2f * t);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
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