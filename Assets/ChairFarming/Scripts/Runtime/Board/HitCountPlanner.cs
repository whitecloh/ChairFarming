using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class HitCountPlanner
    {
        public int SelectHitCount(BallDefinition ball, float launchNormalizedX, System.Random random)
        {
            float centerBias = 1f - Mathf.Abs(launchNormalizedX - 0.5f) * 2f;
            int centerBonus = Mathf.RoundToInt(Mathf.Lerp(-2f, 3f, centerBias));

            int min = Mathf.Max(2, ball.MinHits + Mathf.Min(0, centerBonus));
            int max = Mathf.Max(min, ball.MaxHits + Mathf.Max(0, centerBonus));

            return random.Next(min, max + 1);
        }
    }
}
