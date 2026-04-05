using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;

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

            return _routePlanner.BuildPlan(boardView, ball, launchNormalizedX, fingerIndex, hitCount, routeSeed);
        }
    }
}
