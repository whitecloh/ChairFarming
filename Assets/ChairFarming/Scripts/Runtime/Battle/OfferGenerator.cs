using System;
using System.Collections.Generic;
using System.Linq;
using ChairFarming.Runtime.Core;

namespace ChairFarming.Runtime.Battle
{
    public sealed class OfferGenerator
    {
        public List<BallDefinition> GenerateOffers(ProjectDatabase database, int offerCount, Random random)
        {
            List<BallDefinition> pool = database.Balls
                .Where(ball => ball != null && ball.EnabledInOfferPool)
                .ToList();

            List<BallDefinition> result = new List<BallDefinition>();
            offerCount = Math.Max(1, offerCount);

            while (result.Count < offerCount && pool.Count > 0)
            {
                List<float> weights = pool.Select(ball => (float)Math.Max(1, ball.Weight)).ToList();
                int pickedIndex = WeightedRandomUtility.PickIndex(weights, random);

                result.Add(pool[pickedIndex]);
                pool.RemoveAt(pickedIndex);
            }

            return result;
        }
    }
}
