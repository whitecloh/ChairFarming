using System.Collections.Generic;
using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;

namespace ChairFarming.Runtime.Battle
{
    public sealed class BattleRuntimeState
    {
        public int CurrentLocationIndex;
        public int CurrentEnemyIndex;
        public int CurrentMoney;
        public int CurrentEnemyHp;
        public int RerollCount;
        public int PendingNextBallMultiplier = 1;

        public BallDefinition SelectedBall;
        public float CurrentAimNormalizedX = 0.5f;
        public bool IsDropInProgress;
        public BattlePhase Phase = BattlePhase.None;

        public readonly List<BallDefinition> CurrentOffers = new List<BallDefinition>();

        public DropPlanData PlannedDrop;
        public BallResolutionData LastResolution;

        public void ClearTurnSelection()
        {
            SelectedBall = null;
            PlannedDrop = null;
            CurrentAimNormalizedX = 0.5f;
            IsDropInProgress = false;
        }
    }
}
