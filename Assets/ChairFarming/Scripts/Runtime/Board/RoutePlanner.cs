using System.Collections.Generic;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class RoutePlanner
    {
        private readonly GameBalanceConfig _balanceConfig;

        public RoutePlanner(GameBalanceConfig balanceConfig)
        {
            _balanceConfig = balanceConfig;
        }

        public DropPlanData BuildPlan(
            BoardView boardView,
            BallDefinition ball,
            float launchNormalizedX,
            int targetFingerIndex,
            int targetHitCount,
            int routeSeed)
        {
            System.Random random = new System.Random(routeSeed);

            DropPlanData plan = new DropPlanData
            {
                TargetFingerIndex = targetFingerIndex,
                TargetHitCount = targetHitCount,
                RouteSeed = routeSeed,
                LaunchNormalizedX = launchNormalizedX,
            };

            IReadOnlyList<List<BoardPinPoint>> rows = boardView.Rows;
            FingerSlotView targetFinger = boardView.GetFingerSlot(targetFingerIndex);

            Vector2 startPoint = boardView.GetLaunchWorldPosition(launchNormalizedX);
            plan.Points.Add(new RoutePoint(startPoint, RoutePointType.Move));

            if (rows == null || rows.Count == 0 || targetFinger == null)
            {
                if (targetFinger != null)
                {
                    plan.Points.Add(new RoutePoint(targetFinger.EntryPoint.position, RoutePointType.FingerEntry, -1, targetFingerIndex));
                    plan.Points.Add(new RoutePoint(targetFinger.LandingPoint.position, RoutePointType.FingerLand, -1, targetFingerIndex));
                }

                return plan;
            }

            int[] impactsPerRow = BuildImpactDistribution(rows.Count, targetHitCount, random);

            Vector2 currentPosition = startPoint;
            float targetFingerX = targetFinger.LandingPoint.position.x;

            int passedRows = 0;
            int totalRows = rows.Count;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                List<BoardPinPoint> row = rows[rowIndex];
                if (row == null || row.Count == 0)
                {
                    continue;
                }

                int rowImpacts = Mathf.Max(1, impactsPerRow[rowIndex]);

                float rowProgress = totalRows > 1
                    ? passedRows / (float)(totalRows - 1)
                    : 1f;

                float guidedTargetX = Mathf.Lerp(startPoint.x, targetFingerX, rowProgress);

                float earlyFreedom = Mathf.Lerp(0.15f, 1f, rowProgress);
                float desiredX = Mathf.Lerp(currentPosition.x, guidedTargetX, earlyFreedom);

                float noiseStrength = Mathf.Lerp(1.35f, 0.55f, rowProgress);
                float noise = ((float)random.NextDouble() * 2f - 1f) * _balanceConfig.RouteNoise * noiseStrength;
                desiredX += noise;

                BoardPinPoint chosenPin = PickBestPin(row, currentPosition.x, desiredX);

                float nextDesiredX = rowIndex >= rows.Count - 1
                    ? targetFingerX
                    : Mathf.Lerp(startPoint.x, targetFingerX, (passedRows + 1f) / Mathf.Max(1f, totalRows - 1f));

                int direction = nextDesiredX >= chosenPin.WorldPosition.x ? 1 : -1;

                Vector2 driftPoint = new Vector2(
                    Mathf.Lerp(currentPosition.x, chosenPin.WorldPosition.x, 0.25f),
                    chosenPin.WorldPosition.y + 0.70f);

                Vector2 firstApproach = chosenPin.WorldPosition + new Vector2(-direction * 0.10f, 0.16f);

                if (Vector2.Distance(currentPosition, driftPoint) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(driftPoint, RoutePointType.Move));
                }

                if (Vector2.Distance(driftPoint, firstApproach) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(firstApproach, RoutePointType.Move));
                }

                for (int localImpactIndex = 0; localImpactIndex < rowImpacts; localImpactIndex++)
                {
                    Vector2 impact = chosenPin.WorldPosition + new Vector2(-direction * 0.008f, 0.008f);
                    plan.Points.Add(new RoutePoint(impact, RoutePointType.PinImpact, chosenPin.PinId));

                    bool isLastImpactOnRow = localImpactIndex >= rowImpacts - 1;
                    if (isLastImpactOnRow)
                    {
                        Vector2 rebound = chosenPin.WorldPosition + new Vector2(
                            direction * _balanceConfig.PinReboundSideOffset,
                            _balanceConfig.PinReboundHeight);

                        Vector2 exit = chosenPin.WorldPosition + new Vector2(
                            direction * _balanceConfig.PinReboundExitSideOffset,
                            -_balanceConfig.PinReboundExitDrop);

                        plan.Points.Add(new RoutePoint(rebound, RoutePointType.Move));
                        plan.Points.Add(new RoutePoint(exit, RoutePointType.Move));
                        currentPosition = exit;
                    }
                    else
                    {
                        float repeatedHeight = _balanceConfig.PinReboundHeight + 0.03f * localImpactIndex;

                        Vector2 reboundUp = chosenPin.WorldPosition + new Vector2(
                            direction * (_balanceConfig.PinReboundSideOffset * 0.75f),
                            repeatedHeight);

                        Vector2 reApproach = chosenPin.WorldPosition + new Vector2(
                            -direction * 0.07f,
                            0.10f);

                        plan.Points.Add(new RoutePoint(reboundUp, RoutePointType.Move));
                        plan.Points.Add(new RoutePoint(reApproach, RoutePointType.Move));

                        currentPosition = reApproach;
                    }
                }

                passedRows++;
            }

            Vector2 preFinger = (Vector2)targetFinger.EntryPoint.position + new Vector2(0f, 0.22f);
            Vector2 fingerEntry = targetFinger.EntryPoint.position;
            Vector2 fingerLand = targetFinger.LandingPoint.position;

            plan.Points.Add(new RoutePoint(preFinger, RoutePointType.Move));
            plan.Points.Add(new RoutePoint(fingerEntry, RoutePointType.FingerEntry, -1, targetFingerIndex));
            plan.Points.Add(new RoutePoint(fingerLand, RoutePointType.FingerLand, -1, targetFingerIndex));

            return plan;
        }

        private static int[] BuildImpactDistribution(int rowCount, int targetHitCount, System.Random random)
        {
            int safeRowCount = Mathf.Max(1, rowCount);
            int safeTargetHitCount = Mathf.Max(safeRowCount, targetHitCount);

            int[] impactsPerRow = new int[safeRowCount];

            for (int i = 0; i < safeRowCount; i++)
            {
                impactsPerRow[i] = 1;
            }

            int extraHits = safeTargetHitCount - safeRowCount;

            for (int i = 0; i < extraHits; i++)
            {
                int selectedRow = PickExtraBounceRow(safeRowCount, random);
                impactsPerRow[selectedRow]++;
            }

            return impactsPerRow;
        }

        private static int PickExtraBounceRow(int rowCount, System.Random random)
        {
            if (rowCount <= 1)
            {
                return 0;
            }

            List<float> weights = new List<float>(rowCount);

            for (int i = 0; i < rowCount; i++)
            {
                float normalized = rowCount > 1 ? i / (float)(rowCount - 1) : 0f;
                float centerBias = 1f - Mathf.Abs(normalized - 0.5f) * 2f;
                float lowerBias = Mathf.Lerp(0.8f, 1.25f, normalized);

                weights.Add(Mathf.Max(0.05f, 0.7f + centerBias * 0.6f + lowerBias));
            }

            return WeightedRandomUtility.PickIndex(weights, random);
        }

        private static BoardPinPoint PickBestPin(List<BoardPinPoint> row, float currentX, float desiredX)
        {
            BoardPinPoint best = row[0];
            float bestScore = float.MaxValue;

            for (int i = 0; i < row.Count; i++)
            {
                float score =
                    Mathf.Abs(row[i].WorldPosition.x - desiredX) +
                    Mathf.Abs(row[i].WorldPosition.x - currentX) * 0.12f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = row[i];
                }
            }

            return best;
        }
    }
}