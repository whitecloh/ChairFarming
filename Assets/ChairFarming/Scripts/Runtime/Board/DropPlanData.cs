using System.Collections.Generic;

namespace ChairFarming.Runtime.Board
{
    public sealed class DropPlanData
    {
        public int TargetFingerIndex;
        public int TargetHitCount;
        public int RouteSeed;
        public float LaunchNormalizedX;
        public List<RoutePoint> Points = new List<RoutePoint>();
    }
}
