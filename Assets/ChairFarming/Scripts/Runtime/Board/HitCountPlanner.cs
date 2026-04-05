using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class HitCountPlanner
    {
        public int SelectHitCount(BoardView boardView, GameBalanceConfig balanceConfig, float launchNormalizedX, System.Random random)
        {
            int boardRowCount = boardView != null && boardView.Rows != null
                ? boardView.Rows.Count
                : 0;

            int minimumHits = balanceConfig.UseBoardRowCountAsMinimumHits
                ? Mathf.Max(1, boardRowCount)
                : Mathf.Max(1, balanceConfig.MinimumHitsOverride);

            float centerBias = 1f - Mathf.Abs(launchNormalizedX - 0.5f) * 2f;
            centerBias = Mathf.Clamp01(centerBias);

            int extraMin = Mathf.Max(0, balanceConfig.MinimumExtraBounces);
            int extraMax = Mathf.Max(extraMin, balanceConfig.MaximumExtraBounces);

            int centerBonus = Mathf.RoundToInt(Mathf.Lerp(0f, 2f, centerBias));
            extraMax = Mathf.Max(extraMin, extraMax + centerBonus);

            int extraBounces = random.Next(extraMin, extraMax + 1);

            return minimumHits + extraBounces;
        }
    }
}