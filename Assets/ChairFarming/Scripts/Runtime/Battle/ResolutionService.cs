using System.Collections.Generic;
using System.Text;
using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Battle
{
    public sealed class ResolutionService
    {
        private readonly GameBalanceConfig _config;

        public ResolutionService(GameBalanceConfig config)
        {
            _config = config;
        }

        public BallResolutionData Resolve(
            BallDefinition ball,
            FingerRuntimeInfo finger,
            BattleRuntimeState runtimeState,
            DropPlanData plan,
            int playedCollisions,
            System.Random random)
        {
            if (finger == null)
            {
                finger = new FingerRuntimeInfo
                {
                    Index = 0,
                    ToeType = ToeType.Normal,
                    FootSide = FootSide.None,
                    Multiplier = 1,
                    LandingWeight = 1f,
                };
            }

            int collisions = Mathf.Max(0, playedCollisions);
            int fingerMultiplier = ball != null && ball.UsesFingerMultiplier ? finger.Multiplier : 1;

            int baseResult = collisions * (ball != null ? ball.ValuePerHit : 0);
            int finalResult = baseResult * fingerMultiplier;

            bool procTriggered = false;
            List<string> notes = new List<string>();

            if (ball != null && ball.ToeBonusMultiplier > 1 && ball.ToeBonusType != ToeType.Normal && finger.ToeType == ball.ToeBonusType)
            {
                finalResult *= ball.ToeBonusMultiplier;
                notes.Add("Toe bonus x" + ball.ToeBonusMultiplier);
            }

            if (ball != null && ball.FootBonusMultiplier > 1 && ball.FootBonusSide != FootSide.None && finger.FootSide == ball.FootBonusSide)
            {
                finalResult *= ball.FootBonusMultiplier;
                notes.Add("Foot bonus x" + ball.FootBonusMultiplier);
            }

            if (ball != null && ball.ProcChance > 0f && random.NextDouble() <= ball.ProcChance)
            {
                finalResult *= Mathf.Max(1, ball.ProcMultiplier);
                procTriggered = true;
                notes.Add("Proc x" + ball.ProcMultiplier);
            }

            int moneyDelta = 0;
            int damageDelta = 0;

            if (ball != null)
            {
                switch (ball.Category)
                {
                    case BallCategory.Money:
                        moneyDelta = finalResult;
                        break;

                    case BallCategory.Damage:
                        damageDelta = finalResult;
                        break;

                    case BallCategory.Utility:
                        break;
                }

                switch (ball.SpecialMode)
                {
                    case BallSpecialMode.MoneyToDamageX5:
                        moneyDelta = -collisions;
                        damageDelta = collisions * fingerMultiplier * 5;
                        notes.Add("Money converted to damage");
                        break;

                    case BallSpecialMode.ComboThreshold:
                        damageDelta = finalResult;
                        if (collisions >= _config.ComboThreshold)
                        {
                            damageDelta *= _config.ComboMultiplier;
                            notes.Add("Combo x" + _config.ComboMultiplier);
                        }
                        break;
                }
            }

            int consumedPending = runtimeState.PendingNextBallMultiplier;
            if (consumedPending > 1)
            {
                moneyDelta *= consumedPending;
                damageDelta *= consumedPending;
                notes.Add("Pending x" + consumedPending);
            }

            runtimeState.PendingNextBallMultiplier = 1;

            if (ball != null && ball.SpecialMode == BallSpecialMode.NextBallMultiplier)
            {
                moneyDelta = 0;
                damageDelta = 0;
                runtimeState.PendingNextBallMultiplier = Mathf.Max(
                    1,
                    ball.SpecialIntValue > 0 ? ball.SpecialIntValue : _config.NextBallMultiplier);
                notes.Add("Next ball armed x" + runtimeState.PendingNextBallMultiplier);
            }

            return new BallResolutionData
            {
                BallId = ball != null ? ball.Id : string.Empty,
                Collisions = collisions,
                FingerIndex = finger.Index,
                FingerMultiplier = fingerMultiplier,
                MoneyDelta = moneyDelta,
                DamageDelta = damageDelta,
                ProcTriggered = procTriggered,
                PlannedFingerIndex = plan != null ? plan.TargetFingerIndex : -1,
                PlannedCollisions = plan != null ? plan.TargetHitCount : 0,
                RouteSeed = plan != null ? plan.RouteSeed : 0,
                Notes = string.Join(". ", notes.ToArray()),
            };
        }
    }
}
