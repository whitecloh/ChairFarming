using System.Collections.Generic;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Battle
{
    public sealed class EconomyService
    {
        private readonly GameBalanceConfig _config;

        public EconomyService(GameBalanceConfig config)
        {
            _config = config;
        }

        public int GetRerollCost(int rerollCount)
        {
            int shift = Mathf.Clamp(rerollCount, 0, 20);
            return _config.RerollBaseCost * (1 << shift);
        }

        public bool CanAffordBall(BallDefinition ball, int currentMoney)
        {
            return ball != null && currentMoney >= ball.Cost;
        }

        public bool CanAffordAny(IReadOnlyList<BallDefinition> offers, int currentMoney)
        {
            if (offers == null)
            {
                return false;
            }

            for (int i = 0; i < offers.Count; i++)
            {
                if (offers[i] != null && currentMoney >= offers[i].Cost)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TrySpendBall(BattleRuntimeState state, BallDefinition ball)
        {
            if (!CanAffordBall(ball, state.CurrentMoney))
            {
                return false;
            }

            state.CurrentMoney -= ball.Cost;
            state.CurrentMoney = Mathf.Clamp(state.CurrentMoney, 0, _config.MaxMoney);
            return true;
        }

        public bool TrySpendReroll(BattleRuntimeState state)
        {
            int rerollCost = GetRerollCost(state.RerollCount);
            if (state.CurrentMoney < rerollCost)
            {
                return false;
            }

            state.CurrentMoney -= rerollCost;
            state.CurrentMoney = Mathf.Clamp(state.CurrentMoney, 0, _config.MaxMoney);
            state.RerollCount++;
            return true;
        }

        public void ApplyResolution(BattleRuntimeState state, BallResolutionData data)
        {
            state.CurrentMoney = Mathf.Clamp(state.CurrentMoney + data.MoneyDelta, 0, _config.MaxMoney);
            state.CurrentEnemyHp = Mathf.Max(0, state.CurrentEnemyHp - Mathf.Max(0, data.DamageDelta));
        }

        public bool IsDefeated(BattleRuntimeState state)
        {
            bool canBuyBall = CanAffordAny(state.CurrentOffers, state.CurrentMoney);
            bool canReroll = state.CurrentMoney >= GetRerollCost(state.RerollCount);
            return !canBuyBall && !canReroll;
        }
    }
}
