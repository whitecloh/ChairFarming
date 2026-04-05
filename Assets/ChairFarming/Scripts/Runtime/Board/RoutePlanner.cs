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

            int[] passesPerRow = BuildPassDistribution(rows.Count, targetHitCount, random);

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

                int rowPasses = Mathf.Max(1, passesPerRow[rowIndex]);

                float rowProgress = totalRows > 1
                    ? passedRows / (float)(totalRows - 1)
                    : 1f;

                float guidedTargetX = Mathf.Lerp(startPoint.x, targetFingerX, rowProgress);
                float desiredX = Mathf.Lerp(currentPosition.x, guidedTargetX, Mathf.Lerp(0.22f, 0.92f, rowProgress));

                float noiseStrength = Mathf.Lerp(0.90f, 0.28f, rowProgress);
                float noise = ((float)random.NextDouble() * 2f - 1f) * _balanceConfig.RouteNoise * noiseStrength;
                desiredX += noise;

                GateData chosenGate = PickBestGate(row, currentPosition.x, desiredX);

                float gateWidth = Mathf.Max(0.12f, Vector2.Distance(chosenGate.Left.WorldPosition, chosenGate.Right.WorldPosition));
                float entryHeight = Mathf.Lerp(0.28f, 0.16f, rowProgress);
                float exitDrop = Mathf.Lerp(0.12f, 0.18f, rowProgress);
                float weaveAmount = Mathf.Clamp(gateWidth * 0.18f, 0.03f, 0.08f);

                Vector2 gateCenter = chosenGate.Center;
                Vector2 gateEntry = gateCenter + Vector2.up * entryHeight;
                Vector2 gateExit = gateCenter + Vector2.down * exitDrop;

                Vector2 fallGuide = new Vector2(
                    Mathf.Lerp(currentPosition.x, gateEntry.x, 0.30f),
                    gateEntry.y + 0.18f);

                if (Vector2.Distance(currentPosition, fallGuide) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(fallGuide, RoutePointType.Move));
                }

                if (Vector2.Distance(fallGuide, gateEntry) > 0.01f)
                {
                    plan.Points.Add(new RoutePoint(gateEntry, RoutePointType.Move));
                }

                for (int localPassIndex = 0; localPassIndex < rowPasses; localPassIndex++)
                {
                    if (localPassIndex > 0)
                    {
                        float side = localPassIndex % 2 == 0 ? -1f : 1f;

                        Vector2 loopHigh = gateCenter + new Vector2(side * weaveAmount, entryHeight * 0.72f);
                        Vector2 loopLow = gateCenter + new Vector2(-side * weaveAmount * 0.85f, -exitDrop * 0.28f);

                        plan.Points.Add(new RoutePoint(loopHigh, RoutePointType.Move));
                        plan.Points.Add(new RoutePoint(loopLow, RoutePointType.Move));
                    }

                    plan.Points.Add(new RoutePoint(gateCenter, RoutePointType.GatePass, rowIndex));

                    bool isLastPassOnRow = localPassIndex >= rowPasses - 1;
                    if (isLastPassOnRow)
                    {
                        plan.Points.Add(new RoutePoint(gateExit, RoutePointType.Move));
                        currentPosition = gateExit;
                    }
                    else
                    {
                        float side = localPassIndex % 2 == 0 ? 1f : -1f;
                        Vector2 reEntry = gateCenter + new Vector2(side * weaveAmount, entryHeight * 0.52f);
                        plan.Points.Add(new RoutePoint(reEntry, RoutePointType.Move));
                        currentPosition = reEntry;
                    }
                }

                passedRows++;
            }

            Vector2 preFingerHigh = (Vector2)targetFinger.EntryPoint.position + new Vector2(0f, 0.32f);
            Vector2 preFingerLow = (Vector2)targetFinger.EntryPoint.position + new Vector2(0f, 0.14f);
            Vector2 fingerEntry = targetFinger.EntryPoint.position;
            Vector2 fingerLand = targetFinger.LandingPoint.position;

            plan.Points.Add(new RoutePoint(preFingerHigh, RoutePointType.Move));
            plan.Points.Add(new RoutePoint(preFingerLow, RoutePointType.Move));
            plan.Points.Add(new RoutePoint(fingerEntry, RoutePointType.FingerEntry, -1, targetFingerIndex));
            plan.Points.Add(new RoutePoint(fingerLand, RoutePointType.FingerLand, -1, targetFingerIndex));

            return plan;
        }

        private static int[] BuildPassDistribution(int rowCount, int targetPassCount, System.Random random)
        {
            int safeRowCount = Mathf.Max(1, rowCount);
            int safeTargetPassCount = Mathf.Max(safeRowCount, targetPassCount);

            int[] passesPerRow = new int[safeRowCount];

            for (int i = 0; i < safeRowCount; i++)
            {
                passesPerRow[i] = 1;
            }

            int extraPasses = safeTargetPassCount - safeRowCount;

            for (int i = 0; i < extraPasses; i++)
            {
                int selectedRow = PickExtraPassRow(safeRowCount, random);
                passesPerRow[selectedRow]++;
            }

            return passesPerRow;
        }

        private static int PickExtraPassRow(int rowCount, System.Random random)
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
                float lowerBias = Mathf.Lerp(0.85f, 1.20f, normalized);

                weights.Add(Mathf.Max(0.05f, 0.72f + centerBias * 0.55f + lowerBias));
            }

            return WeightedRandomUtility.PickIndex(weights, random);
        }

        private static GateData PickBestGate(List<BoardPinPoint> row, float currentX, float desiredX)
        {
            List<GateData> gates = BuildGates(row);

            if (gates.Count == 0)
            {
                BoardPinPoint fallback = row[0];
                GateData singleGate = new GateData(fallback, fallback);
                return singleGate;
            }

            GateData best = gates[0];
            float bestScore = float.MaxValue;

            for (int i = 0; i < gates.Count; i++)
            {
                float score =
                    Mathf.Abs(gates[i].Center.x - desiredX) +
                    Mathf.Abs(gates[i].Center.x - currentX) * 0.10f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = gates[i];
                }
            }

            return best;
        }

        private static List<GateData> BuildGates(List<BoardPinPoint> row)
        {
            List<GateData> gates = new List<GateData>();

            if (row == null || row.Count == 0)
            {
                return gates;
            }

            if (row.Count == 1)
            {
                gates.Add(new GateData(row[0], row[0]));
                return gates;
            }

            for (int i = 0; i < row.Count - 1; i++)
            {
                if (row[i] == null || row[i + 1] == null)
                {
                    continue;
                }

                gates.Add(new GateData(row[i], row[i + 1]));
            }

            return gates;
        }

        private readonly struct GateData
        {
            public readonly BoardPinPoint Left;
            public readonly BoardPinPoint Right;
            public readonly Vector2 Center;

            public GateData(BoardPinPoint left, BoardPinPoint right)
            {
                Left = left;
                Right = right;
                Center = (left.WorldPosition + right.WorldPosition) * 0.5f;
            }
        }
    }
}