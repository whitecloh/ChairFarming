using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Game Balance Config", fileName = "GameBalanceConfig")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Offers")]
        [SerializeField] private int offerCount = 3;
        [SerializeField] private int rerollBaseCost = 1;
        [SerializeField] private int maxMoney = 999;

        [Header("Special Rules")]
        [SerializeField] private int comboThreshold = 20;
        [SerializeField] private int comboMultiplier = 3;
        [SerializeField] private int nextBallMultiplier = 10;

        [Header("Route Playback")]
        [SerializeField] private float routeNoise = 0.45f;
        [SerializeField] private float segmentBaseDuration = 0.16f;
        [SerializeField] private float impactApproachDurationMultiplier = 1.05f;
        [SerializeField] private float impactReboundDurationMultiplier = 2.20f;
        [SerializeField] private float normalMoveDurationMultiplier = 1.20f;
        [SerializeField] private float fingerLandingDurationMultiplier = 1.05f;

        [Header("Bounce Feel")]
        [SerializeField] private float pinReboundHeight = 0.46f;
        [SerializeField] private float pinReboundSideOffset = 0.18f;
        [SerializeField] private float pinReboundExitSideOffset = 0.32f;
        [SerializeField] private float pinReboundExitDrop = 0.10f;
        [SerializeField] private float impactPause = 0.035f;
        [SerializeField] private float impactSquashX = 1.16f;
        [SerializeField] private float impactSquashY = 0.84f;
        [SerializeField] private float impactSquashDuration = 0.040f;
        [SerializeField] private float ballSpinSpeed = 210f;
        [SerializeField] private float utilityBallSpinSpeed = 300f;

        [Header("Hit Planning")]
        [SerializeField] private bool useBoardRowCountAsMinimumHits = true;
        [SerializeField] private int minimumHitsOverride = 9;
        [SerializeField] private int minimumExtraBounces = 0;
        [SerializeField] private int maximumExtraBounces = 8;

        [Header("Flow")]
        [SerializeField] private float resolvePause = 0.45f;
        [SerializeField] private float resultPopupDuration = 2.0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool drawRouteGizmos = true;

        public int OfferCount => offerCount;
        public int RerollBaseCost => rerollBaseCost;
        public int MaxMoney => maxMoney;
        public int ComboThreshold => comboThreshold;
        public int ComboMultiplier => comboMultiplier;
        public int NextBallMultiplier => nextBallMultiplier;

        public float RouteNoise => routeNoise;
        public float SegmentBaseDuration => segmentBaseDuration;
        public float ImpactApproachDurationMultiplier => impactApproachDurationMultiplier;
        public float ImpactReboundDurationMultiplier => impactReboundDurationMultiplier;
        public float NormalMoveDurationMultiplier => normalMoveDurationMultiplier;
        public float FingerLandingDurationMultiplier => fingerLandingDurationMultiplier;

        public float PinReboundHeight => pinReboundHeight;
        public float PinReboundSideOffset => pinReboundSideOffset;
        public float PinReboundExitSideOffset => pinReboundExitSideOffset;
        public float PinReboundExitDrop => pinReboundExitDrop;
        public float ImpactPause => impactPause;
        public float ImpactSquashX => impactSquashX;
        public float ImpactSquashY => impactSquashY;
        public float ImpactSquashDuration => impactSquashDuration;
        public float BallSpinSpeed => ballSpinSpeed;
        public float UtilityBallSpinSpeed => utilityBallSpinSpeed;

        public bool UseBoardRowCountAsMinimumHits => useBoardRowCountAsMinimumHits;
        public int MinimumHitsOverride => minimumHitsOverride;
        public int MinimumExtraBounces => minimumExtraBounces;
        public int MaximumExtraBounces => maximumExtraBounces;

        public float ResolvePause => resolvePause;
        public float ResultPopupDuration => resultPopupDuration;
        public bool ShowDebugOverlay => showDebugOverlay;
        public bool DrawRouteGizmos => drawRouteGizmos;

        private void OnValidate()
        {
            offerCount = Mathf.Max(1, offerCount);
            rerollBaseCost = Mathf.Max(0, rerollBaseCost);
            maxMoney = Mathf.Max(1, maxMoney);

            comboThreshold = Mathf.Max(1, comboThreshold);
            comboMultiplier = Mathf.Max(1, comboMultiplier);
            nextBallMultiplier = Mathf.Max(1, nextBallMultiplier);

            routeNoise = Mathf.Clamp(routeNoise, 0f, 3f);
            segmentBaseDuration = Mathf.Max(0.03f, segmentBaseDuration);

            impactApproachDurationMultiplier = Mathf.Clamp(impactApproachDurationMultiplier, 0.4f, 4f);
            impactReboundDurationMultiplier = Mathf.Clamp(impactReboundDurationMultiplier, 0.4f, 5f);
            normalMoveDurationMultiplier = Mathf.Clamp(normalMoveDurationMultiplier, 0.4f, 4f);
            fingerLandingDurationMultiplier = Mathf.Clamp(fingerLandingDurationMultiplier, 0.4f, 4f);

            pinReboundHeight = Mathf.Clamp(pinReboundHeight, 0.08f, 1.5f);
            pinReboundSideOffset = Mathf.Clamp(pinReboundSideOffset, 0.02f, 1.0f);
            pinReboundExitSideOffset = Mathf.Clamp(pinReboundExitSideOffset, 0.02f, 1.2f);
            pinReboundExitDrop = Mathf.Clamp(pinReboundExitDrop, 0.01f, 0.8f);

            impactPause = Mathf.Clamp(impactPause, 0f, 0.15f);
            impactSquashX = Mathf.Clamp(impactSquashX, 1f, 1.5f);
            impactSquashY = Mathf.Clamp(impactSquashY, 0.5f, 1f);
            impactSquashDuration = Mathf.Clamp(impactSquashDuration, 0.005f, 0.15f);

            ballSpinSpeed = Mathf.Clamp(ballSpinSpeed, 0f, 1000f);
            utilityBallSpinSpeed = Mathf.Clamp(utilityBallSpinSpeed, 0f, 1200f);

            minimumHitsOverride = Mathf.Max(1, minimumHitsOverride);
            minimumExtraBounces = Mathf.Max(0, minimumExtraBounces);
            maximumExtraBounces = Mathf.Max(minimumExtraBounces, maximumExtraBounces);

            resolvePause = Mathf.Max(0f, resolvePause);
            resultPopupDuration = Mathf.Max(0.1f, resultPopupDuration);
        }
    }
}