using System.Collections.Generic;
using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    public static class WeightedRandomUtility
    {
        public static int PickIndex(IReadOnlyList<float> weights, System.Random random)
        {
            if (weights == null || weights.Count == 0)
            {
                return 0;
            }

            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                total += Mathf.Max(0f, weights[i]);
            }

            if (total <= 0f)
            {
                return random.Next(0, weights.Count);
            }

            float roll = (float)random.NextDouble() * total;
            float cursor = 0f;

            for (int i = 0; i < weights.Count; i++)
            {
                cursor += Mathf.Max(0f, weights[i]);
                if (roll <= cursor)
                {
                    return i;
                }
            }

            return weights.Count - 1;
        }
    }
}
