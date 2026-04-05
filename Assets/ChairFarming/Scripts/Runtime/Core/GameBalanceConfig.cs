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
        [SerializeField] private float segmentBaseDuration = 0.095f;

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
            segmentBaseDuration = Mathf.Max(0.01f, segmentBaseDuration);
            resolvePause = Mathf.Max(0f, resolvePause);
            resultPopupDuration = Mathf.Max(0.1f, resultPopupDuration);
        }
    }
}
