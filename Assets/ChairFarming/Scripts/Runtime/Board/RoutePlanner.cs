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

            int safeHitCount = Mathf.Clamp(targetHitCount, 2, Mathf.Max(2, rows.Count - 2));
            List<int> selectedRows = PickImpactRows(rows.Count, safeHitCount);

            Vector2 currentPosition = startPoint;
            float targetFingerX = targetFinger.LandingPoint.position.x;

            for (int impactIndex = 0; impactIndex < selectedRows.Count; impactIndex++)
            {
                int rowIndex = selectedRows[impactIndex];
                List<BoardPinPoint> row = rows[rowIndex];
                if (row == null || row.Count == 0)
                {
                    continue;
                }

                float t = (impactIndex + 1f) / (selectedRows.Count + 1f);
                float desiredX = Mathf.Lerp(startPoint.x, targetFingerX, t);

                float noise = ((float)random.NextDouble() * 2f - 1f) * _balanceConfig.RouteNoise * (1f - t * 0.55f);
                desiredX += noise;

                BoardPinPoint chosenPin = PickBestPin(row, currentPosition.x, desiredX);

                float nextDesiredX = impactIndex == selectedRows.Count - 1
                    ? targetFingerX
                    : Mathf.Lerp(startPoint.x, targetFingerX, (impactIndex + 2f) / (selectedRows.Count + 1f));

                int direction = nextDesiredX >= chosenPin.WorldPosition.x ? 1 : -1;

                Vector2 driftPoint = new Vector2(
                    Mathf.Lerp(currentPosition.x, chosenPin.WorldPosition.x, 0.42f),
                    chosenPin.WorldPosition.y + 0.42f);

                Vector2 approach = chosenPin.WorldPosition + new Vector2(-direction * 0.14f, 0.16f);
                Vector2 impact = chosenPin.WorldPosition + new Vector2(-direction * 0.02f, 0.03f);
                Vector2 exit = chosenPin.WorldPosition + new Vector2(direction * 0.16f, -0.15f);

                if (Vector2.Distance(currentPosition, driftPoint) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(driftPoint, RoutePointType.Move));
                }

                if (Vector2.Distance(driftPoint, approach) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(approach, RoutePointType.Move));
                }

                plan.Points.Add(new RoutePoint(impact, RoutePointType.PinImpact, chosenPin.PinId));
                plan.Points.Add(new RoutePoint(exit, RoutePointType.Move));

                currentPosition = exit;
            }

            Vector2 preFinger = (Vector2)targetFinger.EntryPoint.position + new Vector2(0f, 0.28f);
            Vector2 fingerEntry = targetFinger.EntryPoint.position;
            Vector2 fingerLand = targetFinger.LandingPoint.position;

            plan.Points.Add(new RoutePoint(preFinger, RoutePointType.Move));
            plan.Points.Add(new RoutePoint(fingerEntry, RoutePointType.FingerEntry, -1, targetFingerIndex));
            plan.Points.Add(new RoutePoint(fingerLand, RoutePointType.FingerLand, -1, targetFingerIndex));

            return plan;
        }

        private static List<int> PickImpactRows(int totalRows, int targetHitCount)
        {
            List<int> result = new List<int>(Mathf.Max(0, targetHitCount));
            if (targetHitCount <= 0 || totalRows <= 0)
            {
                return result;
            }

            int firstUsableRow = Mathf.Min(1, totalRows - 1);
            int lastUsableRow = Mathf.Max(firstUsableRow, totalRows - 2);

            float step = targetHitCount > 1
                ? (lastUsableRow - firstUsableRow) / (float)(targetHitCount - 1)
                : 0f;

            int lastRow = -1;

            for (int i = 0; i < targetHitCount; i++)
            {
                int row = Mathf.Clamp(Mathf.RoundToInt(firstUsableRow + step * i), firstUsableRow, lastUsableRow);

                if (row <= lastRow)
                {
                    row = Mathf.Min(lastUsableRow, lastRow + 1);
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
                    Mathf.Abs(row[i].WorldPosition.x - currentX) * 0.22f;

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