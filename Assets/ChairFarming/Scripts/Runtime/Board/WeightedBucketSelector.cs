using System.Collections.Generic;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class WeightedBucketSelector
    {
        public int SelectFinger(BoardView boardView, BallDefinition ball, float launchNormalizedX, System.Random random)
        {
            IReadOnlyList<FingerSlotView> fingers = boardView.FingerSlots;
            List<float> finalWeights = new List<float>(fingers.Count);

            for (int i = 0; i < fingers.Count; i++)
            {
                FingerRuntimeInfo finger = fingers[i].GetRuntimeInfo();

                float fingerNormalized = (i + 0.5f) / fingers.Count;
                float distance = Mathf.Abs(fingerNormalized - launchNormalizedX);

                float startBias = Mathf.Pow(Mathf.Max(0.08f, 1f - distance * 1.35f), 1.45f) + 0.04f;
                float ballBias = ball.BucketBias != null && i < ball.BucketBias.Length
                    ? Mathf.Max(0.05f, ball.BucketBias[i])
                    : 1f;
                float baseWeight = Mathf.Max(0.05f, finger.LandingWeight);

                finalWeights.Add(baseWeight * startBias * ballBias);
            }

            return WeightedRandomUtility.PickIndex(finalWeights, random);
        }
    }
}
