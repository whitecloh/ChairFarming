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

            Vector2 startPoint = boardView.PreviewSpawnAnchor != null
                ? boardView.PreviewSpawnAnchor.position
                : boardView.GetLaunchWorldPosition(launchNormalizedX);

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

            List<int> selectedRows = PickImpactRows(rows.Count, targetHitCount);
            Vector2 currentPosition = startPoint;
            float targetFingerX = targetFinger.LandingPoint.position.x;

            for (int impactIndex = 0; impactIndex < selectedRows.Count; impactIndex++)
            {
                int rowIndex = selectedRows[impactIndex];
                List<BoardPinPoint> row = rows[rowIndex];

                float t = (impactIndex + 1f) / (selectedRows.Count + 1f);
                float desiredX = Mathf.Lerp(startPoint.x, targetFingerX, t);
                float noise = ((float)random.NextDouble() * 2f - 1f) * _balanceConfig.RouteNoise * (1f - t * 0.65f);
                desiredX += noise;

                BoardPinPoint chosenPin = PickBestPin(row, currentPosition.x, desiredX);

                float nextDesiredX = impactIndex == selectedRows.Count - 1
                    ? targetFingerX
                    : Mathf.Lerp(startPoint.x, targetFingerX, (impactIndex + 2f) / (selectedRows.Count + 1f));

                int direction = nextDesiredX >= chosenPin.WorldPosition.x ? 1 : -1;

                Vector2 approach = chosenPin.WorldPosition + new Vector2(-direction * 0.22f, 0.28f);
                Vector2 impact = chosenPin.WorldPosition;
                Vector2 exit = chosenPin.WorldPosition + new Vector2(direction * 0.25f, -0.28f);

                if (Vector2.Distance(currentPosition, approach) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(approach, RoutePointType.Move));
                }

                plan.Points.Add(new RoutePoint(impact, RoutePointType.PinImpact, chosenPin.PinId));
                plan.Points.Add(new RoutePoint(exit, RoutePointType.Move));

                currentPosition = exit;
            }

            Vector2 aboveFinger = (Vector2)targetFinger.EntryPoint.position + new Vector2(0f, 0.35f);

            plan.Points.Add(new RoutePoint(aboveFinger, RoutePointType.Move));
            plan.Points.Add(new RoutePoint(targetFinger.EntryPoint.position, RoutePointType.FingerEntry, -1, targetFingerIndex));
            plan.Points.Add(new RoutePoint(targetFinger.LandingPoint.position, RoutePointType.FingerLand, -1, targetFingerIndex));

            return plan;
        }

        private static List<int> PickImpactRows(int totalRows, int targetHitCount)
        {
            List<int> result = new List<int>(Mathf.Max(0, targetHitCount));
            if (targetHitCount <= 0 || totalRows <= 0)
            {
                return result;
            }

            float start = 1f;
            float end = Mathf.Max(start, totalRows - 2f);
            float step = targetHitCount > 1 ? (end - start) / (targetHitCount - 1f) : 0f;

            int lastRow = -1;
            for (int i = 0; i < targetHitCount; i++)
            {
                int row = Mathf.Clamp(Mathf.RoundToInt(start + step * i), 0, totalRows - 1);
                if (row <= lastRow)
                {
                    row = Mathf.Min(totalRows - 1, lastRow + 1);
                }

                result.Add(row);
                lastRow = row;
            }

            return result;
        }

        private static BoardPinPoint PickBestPin(List<BoardPinPoint> row, float currentX, float desiredX)
        {
            BoardPinPoint best = row[0];
            float bestScore = float.MaxValue;

            for (int i = 0; i < row.Count; i++)
            {
                float score =
                    Mathf.Abs(row[i].WorldPosition.x - desiredX) +
                    Mathf.Abs(row[i].WorldPosition.x - currentX) * 0.35f;

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
