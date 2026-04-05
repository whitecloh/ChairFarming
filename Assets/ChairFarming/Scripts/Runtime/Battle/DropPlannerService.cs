using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Battle
{
    public sealed class DropPlannerService
    {
        private readonly WeightedBucketSelector _bucketSelector;
        private readonly HitCountPlanner _hitCountPlanner;
        private readonly RoutePlanner _routePlanner;

        public DropPlannerService(GameBalanceConfig gameConfig)
        {
            _bucketSelector = new WeightedBucketSelector();
            _hitCountPlanner = new HitCountPlanner();
            _routePlanner = new RoutePlanner(gameConfig);
        }

        public DropPlanData BuildPlan(
            BoardView boardView,
            BallDefinition ball,
            float launchNormalizedX,
            System.Random random)
        {
            int routeSeed = random.Next(1, int.MaxValue);

            int fingerIndex = _bucketSelector.SelectFinger(
                boardView,
                ball,
                launchNormalizedX,
                random);

            int hitCount = _hitCountPlanner.SelectHitCount(
                ball,
                launchNormalizedX,
                random);

            int maxUsefulHits = boardView != null && boardView.Rows != null
                ? Mathf.Max(2, boardView.Rows.Count - 2)
                : 2;

            hitCount = Mathf.Clamp(hitCount, 2, maxUsefulHits);

            return _routePlanner.BuildPlan(boardView, ball, launchNormalizedX, fingerIndex, hitCount, routeSeed);
        }
    }
}